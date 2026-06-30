using Coravel.Invocable;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Settings.Dtos;

using Microsoft.Extensions.Options;

namespace Medium.Api.Infrastructure.Scheduler.Jobs;

public class NewsletterQueueJob(
    CommentQueryRepository commentQueryRepository,
    ArticleQueryRepository articleQueryRepository,
    FollowQueryRepository followQueryRepository,
    UserQueryRepository userQueryRepository,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService,
    IOptions<ApplicationSettings> appSettingsOptions,
    IHostApplicationLifetime appLifetime,
    ILogger<NewsletterQueueJob> logger) : IInvocable
{
  public async Task Invoke()
  {
    // Extract native background cancel token for graceful handling
    var cancellationToken = appLifetime.ApplicationStopping;
    try
    {
      logger.LogInformation("NewsletterQueueJob running at {Time}", DateTime.UtcNow);

      // Fetch app settings securely via DI Options pattern
      var appSettings = appSettingsOptions.Value;
      var baseUrl = appSettings.Config.BaseUrl;
      // Calculate current week date boundaries
      var now = DateTime.UtcNow;
      var weekStart = now.AddDays(-(int)now.DayOfWeek);
      var weekEnd = weekStart.AddDays(6);

      // Gather weekly activity statistics asynchronously
      var newArticlesCount = await articleQueryRepository.GetArticlesCountSinceAsync(weekStart, cancellationToken);
      var newCommentsCount = await commentQueryRepository.GetCommentsCountSinceAsync(weekStart, cancellationToken);
      var newFollowersCount = await followQueryRepository.GetTotalFollowsCountAsync(cancellationToken);

      // Retrieve top weekly published articles
      var topArticles = await articleQueryRepository.GetPublishedAsync(1, 5, cancellationToken);
      var topArticleItems = topArticles.Select(article => new NewsletterArticleItem(
          article.Title,
          article.Author.Name,
          $"{baseUrl}/article/{article.Id}"
      )).ToList();

      var weekStartString = weekStart.ToString("MMM dd, yyyy");
      var weekEndString = weekEnd.ToString("MMM dd, yyyy");

      // Define pagination controls
      int currentPage = 1;
      const int pageSize = 100;
      bool hasMoreUsers = true;
      int totalDispatched = 0;

      // Loop through all database records sequentially using a safe chunking window
      while (hasMoreUsers)
      {
        // Ensure the loop breaks immediately if application is shutting down
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("Fetching subscriber batch (based on Follow structure) from database: Page {Page}", currentPage);

        // Fetch only users who follow at least one author using the new repository logic
        var subscribers = await userQueryRepository.GetActiveSubscribersByFollowAsync(currentPage, pageSize, cancellationToken);

        if (subscribers == null || subscribers.Count == 0)
        {
          // Exit the loop when no more records are returned from database
          hasMoreUsers = false;
          break;
        }

        // Build concurrent email tasks for the current chunk list
        var emailTasks = subscribers.Select(async user =>
        {
          try
          {
            // Build customized contextual newsletter template parameters for the recipient
            var emailModel = new NewsletterEmailModel(
                        weekStartString,
                        weekEndString,
                        newArticlesCount,
                        newCommentsCount,
                        newFollowersCount,
                        topArticleItems,
                        baseUrl
                    );

            var emailHtml = await emailTemplateService.RenderTemplateAsync("NewsletterEmail", emailModel);
            await emailService.SendAsync(user.Email, "Weekly Newsletter", emailHtml, cancellationToken);

            // Safely increment counter across multiple parallel threads
            Interlocked.Increment(ref totalDispatched);
          }
          catch (OperationCanceledException)
          {
            logger.LogWarning("Newsletter transmission aborted for {UserEmail} due to host service shutdown.", user.Email);
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "Failed to compile or route weekly newsletter email to user {UserEmail}", user.Email);
          }
        });

        // Execute current batch concurrently before moving to the next database page
        await Task.WhenAll(emailTasks);

        // If the returned batch is smaller than requested pageSize, it means we have reached the end of the table
        if (subscribers.Count < pageSize)
        {
          hasMoreUsers = false;
        }
        else
        {
          // Move target pointer to the next database slice segment
          currentPage++;
        }
      }

      logger.LogInformation("NewsletterQueueJob completed successfully. Total dispatched to {Count} followers across all pages.", totalDispatched);
    }
    catch (OperationCanceledException)
    {
      logger.LogWarning("NewsletterQueueJob execution was interrupted and cancelled gracefully.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Fatal crash occurred inside NewsletterQueueJob process execution context.");
      throw;
    }
  }
}