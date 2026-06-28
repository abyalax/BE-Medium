using System.Text.Json;

using Medium.Api.Infrastructure.Nats.Events;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class EmailServiceResponder : BackgroundService
{
  private readonly NatsConnection _nats;
  private readonly ILogger<EmailServiceResponder> _logger;

  public EmailServiceResponder(
      NatsConnection nats,
      ILogger<EmailServiceResponder> logger)
  {
    _nats = nats;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      await foreach (var msg in _nats.SubscribeAsync<string>("email.send-welcome", cancellationToken: stoppingToken))
      {
        try
        {
          if (string.IsNullOrEmpty(msg.Data))
          {
            continue;
          }

          var request = JsonSerializer.Deserialize<SendWelcomeEmailRequest>(msg.Data)
              ?? throw new InvalidOperationException("Invalid welcome email request payload");

          _logger.LogInformation("Sending welcome email to: {Email}", request.Email);

          await Task.Delay(1000, stoppingToken);

          var response = new SendWelcomeEmailResponse(
            Success: true,
            Message: $"Welcome email sent to {request.Email}"
          );

          var responseJson = JsonSerializer.Serialize(response);
          await msg.ReplyAsync(responseJson, cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Email service error");
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Email Service Responder Error");
    }
  }
}
