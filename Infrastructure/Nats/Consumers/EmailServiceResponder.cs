using System.Text.Json;

using Medium.Api.Domain.Auth.Events;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class EmailServiceResponder : BackgroundService
{
  private readonly INatsConnectionProvider _connectionProvider;
  private readonly ILogger<EmailServiceResponder> _logger;

  public EmailServiceResponder(
      INatsConnectionProvider connectionProvider,
      ILogger<EmailServiceResponder> logger)
  {
    _connectionProvider = connectionProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    // Wait for NATS connection to be ready
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var _ = _connectionProvider.Connection;
        break;
      }
      catch (InvalidOperationException)
      {
        _logger.LogInformation("Waiting for NATS connection to be initialized...");
        await Task.Delay(1000, stoppingToken);
      }
    }
    
    try
    {
      await foreach (var msg in _connectionProvider.Connection.SubscribeAsync<string>("email.send-welcome", cancellationToken: stoppingToken))
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

          var response = new SendWelcomeEmailResponse
          {
            Success = true,
            Message = $"Welcome email sent to {request.Email}"
          };

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