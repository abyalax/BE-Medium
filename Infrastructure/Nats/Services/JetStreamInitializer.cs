using Microsoft.Extensions.Logging;

using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Medium.Api.Infrastructure.Nats.Services;

public static class JetStreamInitializer
{
  public static async Task InitializeJetStreamAsync(NatsConnection nats, ILogger logger)
  {
    try
    {
      var js = new NatsJSContext(nats);

      var config = new StreamConfig("USER_EVENTS", new[] { "user.>" })
      {
        Storage = StreamConfigStorage.File,
        Retention = StreamConfigRetention.Limits,
        MaxAge = (long)TimeSpan.FromDays(7).TotalMilliseconds * 1_000_000,
        Discard = StreamConfigDiscard.Old
      };

      var stream = await js.CreateStreamAsync(config);
      logger.LogInformation("JetStream stream initialized: {StreamName}", stream.Info.Config.Name);
    }
    catch (Exception ex)
    {
      logger.LogWarning("JetStream stream initialization completed/skipped: {Message}", ex.Message);
    }
  }
}