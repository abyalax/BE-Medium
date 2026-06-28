using FluentValidation;

using Medium.Api.Domain.User.Commands;

namespace Medium.Api.Http.Api.Version1.User.Request;

public class DeleteUserRequest : AbstractValidator<DeleteUserCommand>
{
  public DeleteUserRequest()
  {
    RuleFor(x => x.UserId)
       .Must(guid => Guid.TryParse(guid.ToString(), out _))
       .WithMessage("User ID must be valid GUID");
  }
}