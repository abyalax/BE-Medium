using FluentValidation;

using Medium.Api.Domain.ReadingHistory.Command;

namespace Medium.Api.Http.Api.Version1.ReadingHistory.Request;

public class CreateReadingHistoryRequest : AbstractValidator<CreateReadingHistoryCommand>
{
  public CreateReadingHistoryRequest()
  {
    RuleFor(x => x.ArticleId)
      .NotEmpty()
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");

    RuleFor(x => x.DurationSeconds)
      .NotEmpty()
      .GreaterThanOrEqualTo(0).WithMessage("Duration must be non-negative");
  }
}