using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;

using ArgenCash.Application;
using ArgenCash.Infrastructure;
using ArgenCash.Infrastructure.Authentication;

LoadDotEnvVariables();
NormalizeArgenCashEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://192.168.68.108:3000" };
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Paste only the JWT token. Swagger will add the Bearer prefix."
    });

    options.AddSecurityRequirement(swaggerDoc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", swaggerDoc, null)] = new List<string>()
    });
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT settings are missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");

                var authorizationHeader = context.Request.Headers.Authorization.ToString();

                if (authorizationHeader.StartsWith("Bearer Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authorizationHeader[14..].Trim();
                }
                else if (!string.IsNullOrWhiteSpace(authorizationHeader)
                    && !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authorizationHeader.Trim();
                }

                logger.LogInformation(
                    "JWT message received for {Path}. Header present: {HasHeader}. Token extracted: {HasToken}.",
                    context.Request.Path,
                    !string.IsNullOrWhiteSpace(authorizationHeader),
                    !string.IsNullOrWhiteSpace(context.Token));

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");

                logger.LogError(context.Exception, "JWT authentication failed for {Path}.", context.Request.Path);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");

                logger.LogWarning(
                    "JWT challenge for {Path}. Error: {Error}. Description: {Description}.",
                    context.Request.Path,
                    context.Error,
                    context.ErrorDescription);

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void LoadDotEnvVariables()
{
    foreach (var dotenvPath in ResolveDotEnvPaths())
    {
        if (!File.Exists(dotenvPath))
        {
            continue;
        }

        foreach (var rawLine in File.ReadAllLines(dotenvPath))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (string.IsNullOrWhiteSpace(key) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

static IReadOnlyCollection<string> ResolveDotEnvPaths()
{
    var currentDirectory = Directory.GetCurrentDirectory();
    var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Path.Combine(currentDirectory, ".env")
    };

    var directory = new DirectoryInfo(currentDirectory);
    while (directory is not null && !string.Equals(directory.Name, "Backend", StringComparison.OrdinalIgnoreCase))
    {
        directory = directory.Parent;
    }

    if (directory is not null)
    {
        paths.Add(Path.Combine(directory.FullName, ".env"));
        paths.Add(Path.Combine(directory.FullName, "ArgenCash.Api", ".env"));
    }

    return paths;
}

static void NormalizeArgenCashEnvironmentVariables()
{
    var mappings = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["ARGENCASH_DB_CONNECTION"] = "ConnectionStrings__DefaultConnection",
        ["ARGENCASH_JWT_ISSUER"] = "Jwt__Issuer",
        ["ARGENCASH_JWT_AUDIENCE"] = "Jwt__Audience",
        ["ARGENCASH_JWT_SECRET"] = "Jwt__SecretKey",
        ["ARGENCASH_JWT_EXPIRATION_MINUTES"] = "Jwt__ExpirationMinutes",
        ["ARGENCASH_VERIFICATION_TOKEN_SECRET"] = "VerificationToken__SecretKey",
        ["ARGENCASH_VERIFICATION_TOKEN_EXPIRATION_MINUTES"] = "VerificationToken__ExpirationMinutes",
        ["ARGENCASH_SMTP_HOST"] = "Smtp__Host",
        ["ARGENCASH_SMTP_PORT"] = "Smtp__Port",
        ["ARGENCASH_SMTP_USERNAME"] = "Smtp__Username",
        ["ARGENCASH_SMTP_PASSWORD"] = "Smtp__Password",
        ["ARGENCASH_SMTP_FROM_NAME"] = "Smtp__FromName",
        ["ARGENCASH_SMTP_FROM_EMAIL"] = "Smtp__FromEmail",
        ["ARGENCASH_FRONTEND_URL"] = "FrontendUrl",
        ["ARGENCASH_EXCHANGE_RATE_API_BASE_URL"] = "ExchangeRateApi__BaseUrl",
        ["ARGENCASH_EXCHANGE_RATE_SOURCE_NAME"] = "ExchangeRateApi__SourceName",
        ["ARGENCASH_ALLOWED_ORIGIN_0"] = "AllowedOrigins__0"
    };

    foreach (var mapping in mappings)
    {
        var sourceValue = Environment.GetEnvironmentVariable(mapping.Key);
        if (string.IsNullOrWhiteSpace(sourceValue) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(mapping.Value)))
        {
            continue;
        }

        Environment.SetEnvironmentVariable(mapping.Value, sourceValue);
    }
}
