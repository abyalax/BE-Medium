
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;

namespace Medium.Api.Http.Api.Version1.Email;

public static class EmailEndpoints
{
  public static void MapEmailEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/email")
        .WithTags("Email")
        .RequireAuthorization();

    group.MapPost("/send", async (
      MailpitEmailService emailService,
      EmailTemplateService emailTemplateService,
      CancellationToken cancellationToken
      ) =>
    {
      var emailModel = new ArticlePublishedEmailModel(
           "follow.Follower.Name Test",
           "follow.Following.Name Test",
           "@event.Title Test",
           "@event.Slug",
           $"/article/@event.ArticleId"
       );

      var emailHtml = await emailTemplateService.RenderTemplateAsync("ArticlePublishedEmail", emailModel);
      await emailService.SendAsync("test@mail.com", "New Article Published", emailHtml, default);
      return Results.Ok(new { message = "ArticlePublishedEmail sended successfully" });
    })
    .WithName("ArticlePublishedEmail")
    .WithOpenApi();
  }
}