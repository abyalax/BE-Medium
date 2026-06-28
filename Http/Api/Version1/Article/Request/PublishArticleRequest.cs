using FluentValidation;

using Medium.Api.Domain.Article.Commands;

namespace Medium.Api.Http.Api.Version1.Article.Request;

public class PublishArticleRequest : AbstractValidator<PublishArticleCommand>
{
  public PublishArticleRequest()
  {
    RuleFor(x => x.ScheduledAt)
         .Must((request, date) => !date.HasValue || date.Value > DateTimeOffset.UtcNow)
         .WithMessage("Scheduled publish date must be in the future");
  }
}