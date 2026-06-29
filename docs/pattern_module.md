```csharp
// (Include previous CQRS code - Models, DTOs, Commands, Queries, Validators, Handlers, Repositories)

// Events
public abstract class DomainEvent
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class UserRegisteredEvent : DomainEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
}

public class UserLoggedInEvent : DomainEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public DateTime LoginTime { get; set; }
}

public class SendWelcomeEmailRequest
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
}

public class SendWelcomeEmailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

// NATS Event Publisher Service
public interface INatsEventPublisher
{
    Task PublishEventAsync<T>(string subject, T @event) where T : class;
    Task<TResponse> RequestAsync<TRequest, TResponse>(string subject, TRequest request)
        where TRequest : class where TResponse : class;
}

public class NatsEventPublisher : INatsEventPublisher
{
    private readonly INatsConnection _nats;
    private readonly ILogger<NatsEventPublisher> _logger;

    public NatsEventPublisher(INatsConnection nats, ILogger<NatsEventPublisher> logger)
    {
        _nats = nats;
        _logger = logger;
    }

    public async Task PublishEventAsync<T>(string subject, T @event) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(@event);
            await _nats.PublishAsync(subject, json);
            _logger.LogInformation($"Event published: {subject}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to publish event: {subject}, Error: {ex.Message}");
            throw;
        }
    }

    public async Task<TResponse> RequestAsync<TRequest, TResponse>(string subject, TRequest request)
        where TRequest : class where TResponse : class
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var reply = await _nats.RequestAsync(subject, json);
            var response = JsonSerializer.Deserialize<TResponse>(reply.Data.AsString());
            _logger.LogInformation($"Request-Reply completed: {subject}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed request-reply: {subject}, Error: {ex.Message}");
            throw;
        }
    }
}

// JetStream Publisher
public interface IJetStreamEventPublisher
{
    Task PublishToStreamAsync<T>(string streamName, string subject, T @event) where T : class;
}

public class JetStreamEventPublisher : IJetStreamEventPublisher
{
    private readonly INatsConnection _nats;
    private readonly ILogger<JetStreamEventPublisher> _logger;

    public JetStreamEventPublisher(INatsConnection nats, ILogger<JetStreamEventPublisher> logger)
    {
        _nats = nats;
        _logger = logger;
    }

    public async Task PublishToStreamAsync<T>(string streamName, string subject, T @event)
        where T : class
    {
        try
        {
            var js = new NatsJSContext(_nats);
            var json = JsonSerializer.Serialize(@event);
            await js.PublishAsync(subject, json);
            _logger.LogInformation($"Event published to JetStream {streamName}: {subject}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to publish to JetStream: {subject}, Error: {ex.Message}");
            throw;
        }
    }
}

// Modified Command Handlers with NATS Publishing
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly UserStoreRepository _storeRepo;
    private readonly UserQueryRepository _queryRepo;
    private readonly IPasswordService _passwordService;
    private readonly INatsEventPublisher _natsPublisher;
    private readonly IJetStreamEventPublisher _jsPublisher;

    public RegisterCommandHandler(
        UserStoreRepository storeRepo,
        UserQueryRepository queryRepo,
        IPasswordService passwordService,
        INatsEventPublisher natsPublisher,
        IJetStreamEventPublisher jsPublisher
    )
    {
        _storeRepo = storeRepo;
        _queryRepo = queryRepo;
        _passwordService = passwordService;
        _natsPublisher = natsPublisher;
        _jsPublisher = jsPublisher;
    }

    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _queryRepo.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = _passwordService.HashPassword(request.Password)
        };

        await _storeRepo.AddAsync(user);
        await _storeRepo.SaveChangesAsync();

        // Publish event to JetStream
        var @event = new UserRegisteredEvent
        {
            Id = user.Id,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username
        };

        await _jsPublisher.PublishToStreamAsync("USER_EVENTS", "user.registered", @event);

        // Request-Reply: Send welcome email
        var emailRequest = new SendWelcomeEmailRequest
        {
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username
        };

        try
        {
            var emailResponse = await _natsPublisher.RequestAsync<SendWelcomeEmailRequest, SendWelcomeEmailResponse>(
                "email.send-welcome", emailRequest);

            if (!emailResponse.Success)
                throw new InvalidOperationException("Failed to send welcome email");
        }
        catch (Exception ex)
        {
            // Log error but don't fail registration
            System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
        }

        return new UserDto(user.Id, user.Email, user.Username);
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly UserQueryRepository _queryRepo;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IJetStreamEventPublisher _jsPublisher;

    public LoginCommandHandler(
        UserQueryRepository queryRepo,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IJetStreamEventPublisher jsPublisher)
    {
        _queryRepo = queryRepo;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _jsPublisher = jsPublisher;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _queryRepo.GetByEmailAsync(request.Email);
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        var token = _jwtTokenService.GenerateToken(user);

        // Publish login event to JetStream
        var @event = new UserLoggedInEvent
        {
            Id = user.Id,
            UserId = user.Id,
            Email = user.Email,
            LoginTime = DateTime.UtcNow
        };

        await _jsPublisher.PublishToStreamAsync("USER_EVENTS", "user.logged-in", @event);

        return new LoginResponseDto(user.Id, user.Email, user.Username, token);
    }
}

// NATS Subscriber Services

// 1. Push Consumer - Subscribe to user.registered events
public class UserRegisteredPushConsumer : BackgroundService
{
    private readonly INatsConnection _nats;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisteredPushConsumer> _logger;
    private NatsJSContext _js;

    public UserRegisteredPushConsumer(
        INatsConnection nats,
        IServiceProvider serviceProvider,
        ILogger<UserRegisteredPushConsumer> logger)
    {
        _nats = nats;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _js = new NatsJSContext(_nats);

            var consumer = await _js.CreateOrUpdateConsumerAsync(
                "USER_EVENTS",
                new ConsumerConfig
                {
                    Name = "user-registered-push",
                    Durable = "user-registered-push",
                    DeliverSubject = "user.registered.push"
                });

            await foreach (var msg in _js.SubscribeAsync<UserRegisteredEvent>(
                "user.registered.push", cancellationToken: stoppingToken))
            {
                try
                {
                    _logger.LogInformation(
                        $"User Registered Event - UserId: {msg.Data.UserId}, Email: {msg.Data.Email}");

                    // Process event
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // Can inject services here if needed
                        // var service = scope.ServiceProvider.GetRequiredService<IService>();
                    }

                    await msg.AckAsync(cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing user registered event: {ex.Message}");
                    await msg.NakAsync(cancellationToken: stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"User Registered Consumer Error: {ex.Message}");
        }
    }
}

// 2. Pull Consumer - Pull user.logged-in events
public class UserLoggedInPullConsumer : BackgroundService
{
    private readonly INatsConnection _nats;
    private readonly ILogger<UserLoggedInPullConsumer> _logger;
    private NatsJSContext _js;

    public UserLoggedInPullConsumer(
        INatsConnection nats,
        ILogger<UserLoggedInPullConsumer> logger)
    {
        _nats = nats;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _js = new NatsJSContext(_nats);

            await _js.CreateOrUpdateConsumerAsync(
                "USER_EVENTS",
                new ConsumerConfig
                {
                    Name = "user-logged-in-pull",
                    Durable = "user-logged-in-pull",
                    FilterSubject = "user.logged-in"
                });

            // Pull messages every 5 seconds
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var msgs = await _js.FetchAsync<UserLoggedInEvent>(
                        "USER_EVENTS",
                        "user-logged-in-pull",
                        opts: new FetchOpts { MaxMessages = 10, Expires = TimeSpan.FromSeconds(5) },
                        cancellationToken: stoppingToken);

                    await foreach (var msg in msgs)
                    {
                        try
                        {
                            _logger.LogInformation(
                                $"User Logged In Event - UserId: {msg.Data.UserId}, Time: {msg.Data.LoginTime}");

                            await msg.AckAsync(cancellationToken: stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing user logged-in event: {ex.Message}");
                            await msg.NakAsync(cancellationToken: stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error pulling messages: {ex.Message}");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"User Logged In Pull Consumer Error: {ex.Message}");
        }
    }
}

// 3. Request-Reply Handler - Email Service Responder
public class EmailServiceResponder : BackgroundService
{
    private readonly INatsConnection _nats;
    private readonly ILogger<EmailServiceResponder> _logger;

    public EmailServiceResponder(
        INatsConnection nats,
        ILogger<EmailServiceResponder> logger)
    {
        _nats = nats;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _nats.SubscribeAsync("email.send-welcome",
                async args =>
                {
                    try
                    {
                        var request = JsonSerializer.Deserialize<SendWelcomeEmailRequest>(
                            args.Message.Data.AsString());

                        _logger.LogInformation($"Sending welcome email to: {request.Email}");

                        // Simulate email sending
                        await Task.Delay(1000, stoppingToken);

                        var response = new SendWelcomeEmailResponse
                        {
                            Success = true,
                            Message = $"Welcome email sent to {request.Email}"
                        };

                        var responseJson = JsonSerializer.Serialize(response);
                        await args.Message.ReplyAsync(responseJson, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Email service error: {ex.Message}");
                    }
                });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Email Service Responder Error: {ex.Message}");
        }
    }
}

// NATS Initialization Service
public static class NatsServiceCollectionExtensions
{
    public static IServiceCollection AddNatsServices(this IServiceCollection services, IConfiguration config)
    {
        // NATS Connection
        var natsUrl = config.GetValue<string>("Nats:Url") ?? "nats://localhost:4222";
        var opts = NatsOpts.Default with { Url = natsUrl };
        services.AddSingleton<INatsConnection>(sp => new NatsConnection(opts));

        // Publishers
        services.AddScoped<INatsEventPublisher, NatsEventPublisher>();
        services.AddScoped<IJetStreamEventPublisher, JetStreamEventPublisher>();

        // Background Consumers
        services.AddHostedService<UserRegisteredPushConsumer>();
        services.AddHostedService<UserLoggedInPullConsumer>();
        services.AddHostedService<EmailServiceResponder>();

        return services;
    }
}

// Initialize JetStream Streams (run once on startup)
public static class JetStreamInitializer
{
    public static async Task InitializeJetStreamAsync(INatsConnection nats, ILogger logger)
    {
        try
        {
            var js = new NatsJSContext(nats);

            // Create USER_EVENTS stream
            var streamInfo = await js.CreateOrUpdateStreamAsync(new StreamConfig
            {
                Name = "USER_EVENTS",
                Subjects = new[] { "user.>", "user.*" },
                Storage = StreamStorage.File,
                Retention = RetentionPolicy.Limits,
                MaxAge = TimeSpan.FromDays(7),
                Discard = DiscardPolicy.Old
            });

            logger.LogInformation($"JetStream initialized: {streamInfo.Config.Name}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to initialize JetStream: {ex.Message}");
        }
    }
}

// Updated DI Setup
public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuthCqrs(this IServiceCollection services, IConfiguration config)
    {
        // DbContext
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<UserStoreRepository, UserStoreRepository>();

        // Services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Validators
        services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();

        // NATS Services
        services.AddNatsServices(config);

        return services;
    }
}

// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthCqrs(builder.Configuration);

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
        };
    });

var app = builder.Build();

// Initialize JetStream
using (var scope = app.Services.CreateScope())
{
    var nats = scope.ServiceProvider.GetRequiredService<INatsConnection>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await JetStreamInitializer.InitializeJetStreamAsync(nats, logger);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// appsettings.json
/*
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AuthDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-minimum-32-characters-long",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers"
  },
  "Nats": {
    "Url": "nats://localhost:4222"
  }
}
*/
```
