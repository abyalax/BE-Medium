namespace Medium.Api.Infrastructure.Email.Services;

using MailKit.Net.Smtp;
using MailKit.Security;

using Medium.Api.Infrastructure.Email.Config;

using MimeKit;

public sealed class MailpitEmailService(EmailConfiguration configuration)
{
  private readonly EmailConfiguration _configuration = configuration;

  public async Task SendAsync(
    string to,
    string subject,
    string html,
    CancellationToken cancellationToken = default)
  {
    var email = new MimeMessage();

    email.From.Add(
      new MailboxAddress(_configuration.FromName, _configuration.FromEmail)
    );

    email.To.Add(MailboxAddress.Parse(to));
    email.Subject = subject;
    email.Body = new TextPart("html")
    {
      Text = html
    };

    using var smtp = new SmtpClient();

    await smtp.ConnectAsync(
        _configuration.Host,
        _configuration.Port,
        SecureSocketOptions.None,
        cancellationToken);

    if (!string.IsNullOrWhiteSpace(_configuration.Username))
    {
      await smtp.AuthenticateAsync(
        _configuration.Username,
        _configuration.Password,
        cancellationToken
      );
    }

    await smtp.SendAsync(email, cancellationToken);
    await smtp.DisconnectAsync(true, cancellationToken);
  }
}