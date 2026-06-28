using FluentValidation;

using Medium.Api.Domain.User.Commands;

namespace Medium.Api.Http.Api.Version1.User.Request;

public class AssignUserRoleRequest : AbstractValidator<AssignUserRoleCommand>
{
  public AssignUserRoleRequest()
  {
    RuleFor(x => x.RoleIds)
      .NotEmpty()
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .Must(roleIds => roleIds.Distinct().Count() == roleIds.Count)
      .WithMessage("RoleIds must not contain duplicates");
  }
}