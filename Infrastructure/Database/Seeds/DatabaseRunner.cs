using Microsoft.EntityFrameworkCore;

namespace Medium.Api.Infrastructure.Database.Seeds;

public static class DatabaseSeederRunner
{
  /// <summary>
  /// Checks CLI arguments and executes the manual database migration and seeding if requested.
  /// </summary>
  /// <param name="args">The command-line arguments passed to the application.</param>
  /// <param name="builder">The WebApplicationBuilder instance.</param>
  /// <returns>True if the seed script was executed and the app should terminate; otherwise, false.</returns>
  public static async Task<bool> HandleSeedCommandAsync(string[] args, WebApplication app)
  {
    // Check if the specific CLI flag exists
    if (!args.Contains("--seed")) return false;

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    Console.WriteLine("Start To Seeding Database...");

    try
    {
      var context = services.GetRequiredService<ApplicationDbContext>();

      Console.WriteLine("⏳ [1/2] Applying pending database migrations...");
      await context.Database.MigrateAsync();
      Console.WriteLine("✅ Database schema is up-to-date.");

      Console.WriteLine("⏳ [2/2] Executing deterministic data population...");
      await DatabaseSeeder.SeedAsync(services);
      Console.WriteLine("✅ Data seeding executed successfully.");

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("\n🎉 DATABASE SEEDING COMPLETED SUCCESSFULLY!");
      Console.ResetColor();
    }
    catch (Exception ex)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($"\n❌ CRITICAL ERROR DURING SEEDING: {ex.Message}");
      if (ex.InnerException != null)
      {
        Console.WriteLine($"↳ Inner Exception: {ex.InnerException.Message}");
      }
      Console.ResetColor();
    }
    finally
    {
      Console.WriteLine("\n");
    }

    // Return true to indicate that the script ran and the application should stop here
    return true;
  }
}