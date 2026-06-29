using FluentValidation;

using Medium.Api.Domain.Article.Commands;

namespace Medium.Api.Http.Api.Version1.Article.Request;

public class CreateArticleRequest : AbstractValidator<CreateArticleCommand>
{
  public CreateArticleRequest()
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

    RuleForEach(x => x.TagIds)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .When(x => x.TagIds != null && x.TagIds.Count > 0)
      .WithMessage("Tag IDs must be valid GUIDs");
  }
}