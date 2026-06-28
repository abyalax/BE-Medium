namespace Medium.Api.Infrastructure.Settings;

public class AppSettings
{
  public JwtSettings Jwt { get; set; } = new();
  public NatsSettings Nats { get; set; } = new();
  public EmailSettings Email { get; set; } = new();
  public MinioSettings Minio { get; set; } = new();
  public RedisSettings Redis { get; set; } = new();
  public PaginationSettings Pagination { get; set; } = new();
  public StorageSettings Storage { get; set; } = new();
}

public class JwtSettings
{
  public string Key { get; set; } = string.Empty;
  public string Issuer { get; set; } = string.Empty;
  public string Audience { get; set; } = string.Empty;
  public int AccessTokenExpirationMinutes { get; set; }
  public int RefreshTokenExpirationDays { get; set; }
}

public class NatsSettings
{
  public string Url { get; set; } = string.Empty;
}

public class EmailSettings
{
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; }
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public bool EnableSsl { get; set; }
  public string FromEmail { get; set; } = string.Empty;
  public string FromName { get; set; } = string.Empty;
}

public class MinioSettings
{
  public string Endpoint { get; set; } = string.Empty;
  public string AccessKey { get; set; } = string.Empty;
  public string SecretKey { get; set; } = string.Empty;
  public bool UseSsl { get; set; }
  public string Region { get; set; } = string.Empty;
  public string BucketName { get; set; } = string.Empty;
}

public class RedisSettings
{
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; }
  public string Password { get; set; } = string.Empty;
}

public class PaginationSettings
{
  public int DefaultPage { get; set; } = 1;
  public int DefaultPageSize { get; set; } = 10;
  public int MaxPageSize { get; set; } = 100;
}

public class StorageSettings
{
  public string Provider { get; set; } = string.Empty;
  public string AvatarBucket { get; set; } = string.Empty;
  public string ArticleCoverBucket { get; set; } = string.Empty;
}