using FluentValidation;

namespace Medium.Api.Domain.Article.Dtos;

public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.CoverImageUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.CoverImageUrl))
            .WithMessage("Cover image URL must be a valid URL");
    }
}

public class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
{
    public UpdateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required");

        RuleFor(x => x.CoverImageUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.CoverImageUrl))
            .WithMessage("Cover image URL must be a valid URL");
    }
}

public class PublishArticleRequestValidator : AbstractValidator<PublishArticleRequest>
{
    public PublishArticleRequestValidator()
    {
        RuleFor(x => x.ScheduledAt)
            .Must((request, date) => !date.HasValue || date.Value > DateTimeOffset.UtcNow)
            .WithMessage("Scheduled publish date must be in the future");
    }
}
