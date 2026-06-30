using FluentValidation;

using Medium.Api.Infrastructure.Settings.Dtos;

namespace Medium.Api.Infrastructure.Settings.Validation;

public class ApplicationSettingsValidation : AbstractValidator<ApplicationSettings>
{
  public ApplicationSettingsValidation()
  {
    RuleFor(x => x.Config).NotNull().SetValidator(new ConfigSettingsValidator());
    RuleFor(x => x.Jwt).NotNull().SetValidator(new JwtSettingsValidator());
    RuleFor(x => x.Nats).NotNull().SetValidator(new NatsSettingsValidator());
    RuleFor(x => x.Email).NotNull().SetValidator(new EmailSettingsValidator());
    RuleFor(x => x.Minio).NotNull().SetValidator(new MinioSettingsValidator());
    RuleFor(x => x.Redis).NotNull().SetValidator(new RedisSettingsValidator());
    RuleFor(x => x.Pagination).NotNull().SetValidator(new PaginationSettingsValidator());
    RuleFor(x => x.Storage).NotNull().SetValidator(new StorageSettingsValidator());
  }
}

public class ConfigSettingsValidator : AbstractValidator<ConfigSettings>
{
  public ConfigSettingsValidator()
  {
    RuleFor(x => x.BaseUrl).NotEmpty().WithMessage("Config BaseUrl is missing.");
  }
}

public class JwtSettingsValidator : AbstractValidator<JwtSettings>
{
  public JwtSettingsValidator()
  {
    RuleFor(x => x.Key).NotEmpty().MinimumLength(16).WithMessage("JWT Key must be at least 16 characters.");
    RuleFor(x => x.Issuer).NotEmpty().WithMessage("JWT Issuer is missing.");
    RuleFor(x => x.Audience).NotEmpty().WithMessage("JWT Audience is missing.");
    RuleFor(x => x.AccessTokenExpirationMinutes).GreaterThan(0).WithMessage("JWT AccessTokenExpirationMinutes must be greater than 0.");
    RuleFor(x => x.RefreshTokenExpirationDays).GreaterThan(0).WithMessage("JWT RefreshTokenExpirationDays must be greater than 0.");
  }
}

public class NatsSettingsValidator : AbstractValidator<NatsSettings>
{
  public NatsSettingsValidator()
  {
    RuleFor(x => x.Url).NotEmpty().WithMessage("NATS Url configuration is missing.");
  }
}

public class EmailSettingsValidator : AbstractValidator<EmailSettings>
{
  public EmailSettingsValidator()
  {
    RuleFor(x => x.Host).NotEmpty().WithMessage("Email Host configuration is missing.");
    RuleFor(x => x.Port).GreaterThan(0).WithMessage("Email Port must be a valid port number.");
    RuleFor(x => x.Username).NotEmpty().WithMessage("Email Username is missing.");
    RuleFor(x => x.Password).NotEmpty().WithMessage("Email Password is missing.");
    RuleFor(x => x.FromEmail).NotEmpty().EmailAddress().WithMessage("Email FromEmail must be a valid email address.");
    RuleFor(x => x.FromName).NotEmpty().WithMessage("Email FromName is missing.");
  }
}

public class MinioSettingsValidator : AbstractValidator<MinioSettings>
{
  public MinioSettingsValidator()
  {
    RuleFor(x => x.Endpoint).NotEmpty().WithMessage("Minio Endpoint is missing.");
    RuleFor(x => x.AccessKey).NotEmpty().WithMessage("Minio AccessKey is missing.");
    RuleFor(x => x.SecretKey).NotEmpty().WithMessage("Minio SecretKey is missing.");
    RuleFor(x => x.BucketName).NotEmpty().WithMessage("Minio BucketName is missing.");
  }
}

public class RedisSettingsValidator : AbstractValidator<RedisSettings>
{
  public RedisSettingsValidator()
  {
    RuleFor(x => x.Host).NotEmpty().WithMessage("Redis Host configuration is missing.");
    RuleFor(x => x.Port).GreaterThan(0).WithMessage("Redis Port must be greater than 0.");
  }
}

public class PaginationSettingsValidator : AbstractValidator<PaginationSettings>
{
  public PaginationSettingsValidator()
  {
    RuleFor(x => x.DefaultPage).GreaterThan(0).WithMessage("Pagination DefaultPage must be greater than 0.");
    RuleFor(x => x.DefaultPageSize).GreaterThan(0).WithMessage("Pagination DefaultPageSize must be greater than 0.");
    RuleFor(x => x.MaxPageSize).GreaterThanOrEqualTo(x => x.DefaultPageSize).WithMessage("Pagination MaxPageSize must be greater than or equal to DefaultPageSize.");
  }
}

public class StorageSettingsValidator : AbstractValidator<StorageSettings>
{
  public StorageSettingsValidator()
  {
    RuleFor(x => x.Provider).NotEmpty().WithMessage("Storage Provider configuration is missing.");
    RuleFor(x => x.AvatarBucket).NotEmpty().WithMessage("Storage AvatarBucket is missing.");
    RuleFor(x => x.ArticleCoverBucket).NotEmpty().WithMessage("Storage ArticleCoverBucket is missing.");
  }
}