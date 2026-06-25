using System.Text.RegularExpressions;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Services;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public class CommentCreatedEventHandler(
    ILogger<CommentCreatedEventHandler> logger,
    NotificationService notificationService,
    ArticleRepository articleRepository,
    UserRepository userRepository,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService
  ) : IEventHandler<CommentCreatedEvent>
{
  private readonly ILogger<CommentCreatedEventHandler> _logger = logger;
  private readonly NotificationService _notificationService = notificationService;
  private readonly ArticleRepository _articleRepository = articleRepository;
  private readonly UserRepository _userRepository = userRepository;
  private readonly MailpitEmailService _emailService = emailService;
  private readonly EmailTemplateService _emailTemplateService = emailTemplateService;

  public async Task HandleAsync(CommentCreatedEvent @event)
  {
    try
    {
      _logger.LogInformation("Handling CommentCreated: {CommentId} on {ArticleId}", @event.CommentId, @event.ArticleId);

      // Get article to notify author
      var article = await _articleRepository.GetByIdAsync(Guid.Parse(@event.ArticleId), default);
      if (article == null)
      {
        _logger.LogWarning("Article {ArticleId} not found for comment {CommentId}", @event.ArticleId, @event.CommentId);
        return;
      }

      // Get commenter info
      var commenter = await _userRepository.GetByIdAsync(Guid.Parse(@event.UserId), default);

      // Notify article author
      if (article.AuthorId != Guid.Parse(@event.UserId))
      {
        await _notificationService.CreateAsync(new CreateNotificationRequest(
            article.AuthorId.ToString(),
            "New Comment on Your Article",
            $"Someone commented on your article: {@event.Content}",
            NotificationType.CommentCreated,
            @event.CommentId,
            $"/articles/{@event.ArticleId}#comment-{@event.CommentId}"
        ), default);

        if (commenter != null)
        {
          var emailModel = new CommentCreatedEmailModel(
              article.Author.Name,
              commenter.Name,
              article.Title,
              @event.Content,
              $"/articles/{@event.ArticleId}#comment-{@event.CommentId}"
          );

          var emailHtml = await _emailTemplateService.RenderTemplateAsync("CommentCreatedEmail", emailModel);
          await _emailService.SendAsync(article.Author.Email, "New Comment on Your Article", emailHtml, default);
        }
      }

      // Check for mentions
      var mentions = ExtractMentions(@event.Content);
      foreach (var mentionedUserId in mentions)
      {
        if (mentionedUserId != @event.UserId && mentionedUserId != article.AuthorId.ToString())
        {
          await _notificationService.CreateAsync(new CreateNotificationRequest(
              mentionedUserId,
              "You were mentioned in a comment",
              $"Someone mentioned you in a comment: {@event.Content}",
              NotificationType.Mention,
              @event.CommentId,
              $"/articles/{@event.ArticleId}#comment-{@event.CommentId}"
          ), default);
        }
      }

      _logger.LogInformation("CommentCreated event processed for {CommentId}", @event.CommentId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling CommentCreated");
      throw;
    }
  }

  private static List<string> ExtractMentions(string content)
  {
    // Extract mentions like @userId or @username
    var mentions = new List<string>();
    var regex = new Regex(@"@([a-zA-Z0-9_-]+)");
    var matches = regex.Matches(content);

    foreach (Match match in matches)
    {
      mentions.Add(match.Groups[1].Value);
    }

    return mentions;
  }
}