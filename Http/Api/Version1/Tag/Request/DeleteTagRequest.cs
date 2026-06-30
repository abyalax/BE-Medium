using FluentValidation;

using Medium.Api.Domain.Tag.Commands;

namespace Medium.Api.Http.Api.Version1.Tag.Request;

public class DeleteTagValidator : AbstractValidator<DeleteTagCommand>
{
  public DeleteTagValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty().WithMessage("Id is required");
  }
}