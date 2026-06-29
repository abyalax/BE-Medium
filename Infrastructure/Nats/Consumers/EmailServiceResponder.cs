using System.Text.Json;

using Medium.Api.Domain.Auth.Events;
using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.Logging;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class EmailServiceResponder : IManuallyStartableService
{
  private readonly INatsConnectionProvider _connectionProvider;
  private readonly ILogger<EmailServiceResponder> _logger;
  private CancellationTokenSource? _cancellationTokenSource;

  public EmailServiceResponder(
      INatsConnectionProvider connectionProvider,
      ILogger<EmailServiceResponder> logger)
  {
    _connectionProvider = connectionProvider;
    _logger = logger;
  }

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    try
    {
      await foreach (var msg in _connectionProvider.Connection.SubscribeAsync<string>("email.send-welcome", cancellationToken: cancellationToken))
      {
        try
        {
          if (string.IsNullOrEmpty(msg.Data)) continue;

          var request = JsonSerializer.Deserialize<SendWelcomeEmailRequest>(msg.Data)
              ?? throw new InvalidOperationException("Invalid welcome email request payload");

          _logger.LogInformation("Sending welcome email to: {Email}", request.Email);

          await Task.Delay(1000, cancellationToken);

          var response = new SendWelcomeEmailResponse
          {
            Success = true,
            Message = $"Welcome email sent to {request.Email}"
          };

          var responseJson = JsonSerializer.Serialize(response);
          await msg.ReplyAsync(responseJson, cancellationToken: cancellationToken);
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

  public async Task StopAsync(CancellationToken cancellationToken = default)
  {
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
    await Task.CompletedTask;
  }
}
