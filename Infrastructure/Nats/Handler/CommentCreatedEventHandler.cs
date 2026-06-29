using MediatR;

using Medium.Api.Common.Utils;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Notification.Commands;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Enums;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public class CommentCreatedEventHandler(
    ILogger<CommentCreatedEventHandler> logger,
    IMediator mediator,
    ArticleQueryRepository articleQueryRepository,
    UserQueryRepository userQueryRepository,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService
) : IEventHandler<CommentCreatedEvent>
{
  public async Task HandleAsync(CommentCreatedEvent @event, CancellationToken cancellationToken = default)
  {
    try
    {
      logger.LogInformation("Handling CommentCreated via NATS: {CommentId} on {ArticleId}", @event.CommentId, @event.ArticleId);

      var article = await articleQueryRepository.GetByIdAsync(Guid.Parse(@event.ArticleId), cancellationToken);
      if (article == null)
      {
        logger.LogWarning("Article {ArticleId} not found for comment {CommentId}", @event.ArticleId, @event.CommentId);
        return;
      }

      var notificationCommands = new List<CreateNotificationCommand>();
      var commentUrl = $"/article/{@event.ArticleId}#comment-{@event.CommentId}";

      if (article.AuthorId.ToString() != @event.UserId)
      {
        notificationCommands.Add(new CreateNotificationCommand(
            article.AuthorId,
            "New Comment on Your Article",
            $"Someone commented on your article: {@event.Content}",
            NotificationType.CommentCreated,
            Guid.Parse(@event.CommentId),
            commentUrl
        ));

        _ = Task.Run(async () =>
        {
          try
          {
            var commenter = await userQueryRepository.GetByIdAsync(Guid.Parse(@event.UserId), cancellationToken);
            if (commenter != null)
            {
              var emailModel = new CommentCreatedEmailModel(
                          article.Author.Name,
                          commenter.Name,
                          article.Title,
                          @event.Content,
                          commentUrl
                      );

              var emailHtml = await emailTemplateService.RenderTemplateAsync("CommentCreatedEmail", emailModel);
              await emailService.SendAsync(article.Author.Email, "New Comment on Your Article", emailHtml, cancellationToken);
            }
          }
          catch (Exception ex)
          {
            logger.LogError(ex, "Failed to send comment notification email to author: {AuthorId}", article.AuthorId);
          }
        }, cancellationToken);
      }

      var mentions = Utils.ExtractMentions(@event.Content);
      foreach (var mentionedUserId in mentions)
      {
        if (mentionedUserId != @event.UserId && mentionedUserId != article.AuthorId.ToString())
        {
          notificationCommands.Add(new CreateNotificationCommand(
              Guid.Parse(mentionedUserId),
              "You were mentioned in a comment",
              $"Someone mentioned you in a comment: {@event.Content}",
              NotificationType.Mention,
              Guid.Parse(@event.CommentId),
              commentUrl
          ));
        }
      }

      if (notificationCommands.Count != 0)
      {
        var bulkNotificationCommand = new CreateRangeNotificationCommand(notificationCommands);
        await mediator.Send(bulkNotificationCommand, cancellationToken);
      }

      logger.LogInformation("CommentCreated event successfully processed for {CommentId}", @event.CommentId);
    }
    catch (OperationCanceledException)
    {
      logger.LogWarning("CommentCreated event processing was canceled gracefully.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error handling CommentCreated inside NATS Handler");
      throw;
    }
  }
}