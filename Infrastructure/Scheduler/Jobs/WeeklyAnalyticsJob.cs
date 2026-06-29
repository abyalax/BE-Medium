using Coravel.Invocable;

using MediatR;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.Notification.Commands;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Enums;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;

namespace Medium.Api.Infrastructure.Scheduler.Jobs;

public class WeeklyAnalyticsJob(
    FollowQueryRepository followQueryRepository,
    ArticleQueryRepository articleQueryRepository,
    CommentQueryRepository commentQueryRepository,
    UserQueryRepository userQueryRepository,
    IMediator mediator,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService,
    IHostApplicationLifetime appLifetime,
    ILogger<WeeklyAnalyticsJob> logger
) : IInvocable
{
  public async Task Invoke()
  {

    var cancellationToken = appLifetime.ApplicationStopping;

    try
    {
      logger.LogInformation("WeeklyAnalyticsJob running at {Time}", DateTime.UtcNow);

      // Calculate weekly statistics
      var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
      // Get total user count (approximate by counting follows)
      var totalFollows = await followQueryRepository.GetTotalFollowsCountAsync(cancellationToken);
      // Get new articles this week
      var newArticlesCount = await articleQueryRepository.GetArticlesCountSinceAsync(oneWeekAgo, cancellationToken);
      // Get new comments this week
      var newCommentsCount = await commentQueryRepository.GetCommentsCountSinceAsync(oneWeekAgo, cancellationToken);

      var weeklyStats = new Dictionary<string, object>
      {
        ["totalFollows"] = totalFollows,
        ["newArticles"] = newArticlesCount,
        ["newComments"] = newCommentsCount,
        ["period"] = "week",
        ["calculatedAt"] = DateTime.UtcNow
      };

      logger.LogInformation("WeeklyAnalyticsJob completed calculation. Stats: {@Stats}", weeklyStats);

      var adminUsers = await userQueryRepository.ListAsync(1, 10, cancellationToken);
      if (adminUsers == null || adminUsers.Count == 0)
      {
        logger.LogWarning("No admin users found to receive the weekly report.");
        return;
      }

      var notificationCommands = new List<CreateNotificationCommand>();
      var messageContent = $"This week: {newArticlesCount} new articles, {newCommentsCount} new comments, {totalFollows} total follows";

      foreach (var admin in adminUsers)
      {
        notificationCommands.Add(new CreateNotificationCommand(
            admin.Id,
            "Weekly Analytics Report",
            messageContent,
            NotificationType.ArticlePublished,
            null,
            "/analytics"
        ));
      }

      if (notificationCommands.Count != 0)
      {
        var bulkNotificationCommand = new CreateRangeNotificationCommand(notificationCommands);
        await mediator.Send(bulkNotificationCommand, cancellationToken);
      }

      var weekStart = oneWeekAgo.ToString("MMM dd, yyyy");
      var weekEnd = DateTime.UtcNow.ToString("MMM dd, yyyy");

      var topArticles = await articleQueryRepository.GetPublishedAsync(1, 5, cancellationToken);
      var topArticleItems = topArticles.Select(article => new NewsletterArticleItem(
          article.Title,
          article.Author.Name,
          $"/article/{article.Id}"
      )).ToList();

      var emailTasks = adminUsers.Select(async admin =>
      {
        try
        {
          var emailModel = new NewsletterEmailModel(
                    weekStart,
                    weekEnd,
                    newArticlesCount,
                    newCommentsCount,
                    totalFollows,
                    topArticleItems,
                    "https://medium-clone.com"
                );

          var emailHtml = await emailTemplateService.RenderTemplateAsync("NewsletterEmail", emailModel);
          await emailService.SendAsync(admin.Email, "Weekly Analytics Report", emailHtml, cancellationToken);

          logger.LogInformation("Analytics report email successfully sent to admin: {AdminEmail}", admin.Email);
        }
        catch (OperationCanceledException)
        {
          logger.LogWarning("Email sending canceled for admin: {AdminEmail} due to application shutdown.", admin.Email);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Failed to send analytics email to admin: {AdminEmail}", admin.Email);
        }
      });

      await Task.WhenAll(emailTasks);

      logger.LogInformation("WeeklyAnalyticsJob fully processed.");
    }
    catch (OperationCanceledException)
    {
      logger.LogWarning("WeeklyAnalyticsJob execution was canceled gracefully due to application shutdown.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error occurred inside WeeklyAnalyticsJob");
      throw;
    }
  }
}