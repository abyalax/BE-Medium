using FluentValidation;
using Medium.Api.Domain.Tag.Dtos;

namespace Medium.Api.Domain.Tag.Dtos;

public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9\s\-]+$").WithMessage("Tag name can only contain letters, numbers, spaces, and hyphens");
    }
}

public class UpdateTagRequestValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9\s\-]+$").WithMessage("Tag name can only contain letters, numbers, spaces, and hyphens");
    }
}
