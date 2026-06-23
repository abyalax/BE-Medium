using FluentValidation;
using Medium.Api.Domain.ReadingHistory.Dtos;

namespace Medium.Api.Domain.ReadingHistory.Dtos;

public class CreateReadingHistoryRequestValidator : AbstractValidator<CreateReadingHistoryRequest>
{
    public CreateReadingHistoryRequestValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Article ID is required");

        RuleFor(x => x.DurationSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Duration must be non-negative");
    }
}
