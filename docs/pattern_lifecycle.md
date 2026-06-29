```csharp
// Infrastructure Layer
public interface IDataStore
{
Task ConnectAsync();
Task DisconnectAsync();
Task<string> QueryAsync(string sql);
}

public class DataStore : IDataStore
{
private SqlConnection? _connection;
private readonly string _connectionString;

    public DataStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task ConnectAsync()
    {
        _connection = new SqlConnection(_connectionString);
        await _connection.OpenAsync();
        Console.WriteLine("✓ Database connected");
    }

    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
            Console.WriteLine("✗ Database disconnected");
        }
    }

    public async Task<string> QueryAsync(string sql)
    {
        var cmd = _connection!.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString() ?? "null";
    }

}

public interface ICacheProvider
{
Task InitializeAsync();
void Set(string key, string value);
string? Get(string key);
}

public class CacheProvider : ICacheProvider
{
private readonly Dictionary<string, string> _cache = new();

    public async Task InitializeAsync()
    {
        // Simulasi koneksi ke cache server
        await Task.Delay(100);
        Console.WriteLine("✓ Cache provider initialized");
    }

    public void Set(string key, string value) => _cache[key] = value;
    public string? Get(string key) => _cache.TryGetValue(key, out var value) ? value : null;

}

// Application Service
public class ApplicationModule : IAsyncDisposable
{
private readonly IDataStore _dataStore;
private readonly ICacheProvider _cacheProvider;

    public ApplicationModule(IDataStore dataStore, ICacheProvider cacheProvider)
    {
        _dataStore = dataStore;
        _cacheProvider = cacheProvider;
        Console.WriteLine("[OnModuleCreate] Module dependencies injected");
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup
        await _dataStore.DisconnectAsync();
    }

}

// HostedService dengan 3 fase lifecycle
public class ApplicationLifecycleService : IHostedService, IAsyncDisposable
{
private readonly IServiceProvider _serviceProvider;
private ApplicationModule? _module;

    public ApplicationLifecycleService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // FASE 1: Startup - Connect Infrastructure
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n--- [OnStartup] Memulai aplikasi ---");

        try
        {
            // Create scope untuk DI
            using var scope = _serviceProvider.CreateAsyncScope();

            // FASE 1A: Initialize dependencies
            var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();
            var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();

            // FASE 1B: Connect to infrastructure
            await dataStore.ConnectAsync();
            await cacheProvider.InitializeAsync();

            // FASE 1C: Create application module (setelah semua infra siap)
            _module = scope.ServiceProvider.GetRequiredService<ApplicationModule>();

            Console.WriteLine("[OnStartup] ✓ Aplikasi siap\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OnStartup] ✗ Error: {ex.Message}");
            throw;
        }
    }

    // FASE 3: Shutdown - Close Infrastructure
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n--- [OnShutdown] Menutup aplikasi ---");

        if (_module != null)
        {
            await _module.DisposeAsync();
        }

        Console.WriteLine("[OnShutdown] ✓ Aplikasi tertutup\n");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
        }
    }

}

// Program.cs
public class Program
{
public static async Task Main(string[] args)
{
var builder = WebApplicationBuilder.CreateBuilder(args);

        // DI Registration
        builder.Services.AddSingleton<IDataStore>(
            new DataStore("Server=localhost;Database=MyDb;Trusted_Connection=true;")
        );

        builder.Services.AddSingleton<ICacheProvider, CacheProvider>();

        builder.Services.AddScoped<ApplicationModule>();

        // Register lifecycle service
        builder.Services.AddHostedService<ApplicationLifecycleService>();

        var app = builder.Build();

        app.MapGet("/", async () =>
        {
            var dataStore = app.Services.GetRequiredService<IDataStore>();
            var result = await dataStore.QueryAsync("SELECT 'Hello from DB'");
            return new { message = result, timestamp = DateTime.Now };
        });

        await app.RunAsync();
    }

}
```
