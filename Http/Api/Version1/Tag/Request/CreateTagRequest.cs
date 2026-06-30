using FluentValidation;

using Medium.Api.Domain.Tag.Commands;

namespace Medium.Api.Http.Api.Version1.Tag.Request;

public class CreateTagValidator : AbstractValidator<CreateTagCommand>
{
  public CreateTagValidator()
  {
    RuleFor(x => x.Request.Name)
      .NotEmpty().WithMessage("Name is required")
      .MinimumLength(2).WithMessage("Name must be at least 2 characters")
      .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");
  }
}