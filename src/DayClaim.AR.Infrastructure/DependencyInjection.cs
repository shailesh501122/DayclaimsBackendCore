using System.Text;
using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Infrastructure.Caching;
using DayClaim.AR.Infrastructure.Common;
using DayClaim.AR.Infrastructure.Messaging;
using DayClaim.AR.Infrastructure.Persistence;
using DayClaim.AR.Infrastructure.Security;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace DayClaim.AR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.SectionName));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IFieldEncryptionService, FieldEncryptionService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        services.AddMassTransit(bus =>
        {
            bus.UsingRabbitMq((context, cfg) =>
            {
                // VirtualHost defaults to "/" for local Docker Compose's stock RabbitMQ image,
                // but hosted brokers (e.g. CloudAMQP) assign a per-account vhost — must be
                // configurable, not hardcoded, or every publish fails against a real provider.
                var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
                var port = configuration.GetValue<ushort?>("RabbitMq:Port");
                var useTls = configuration.GetValue("RabbitMq:UseTls", false);

                cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", port ?? 5672, virtualHost, h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                    if (useTls)
                    {
                        h.UseSsl(ssl => ssl.Protocol = System.Security.Authentication.SslProtocols.Tls12);
                    }
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        var signingKey = jwtSection["SigningKey"] ?? string.Empty;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase);
                // Without this, JwtSecurityTokenHandler silently remaps short claim
                // types (e.g. "sub") to legacy XML-namespace URIs on the resulting
                // ClaimsPrincipal, so code reading JwtRegisteredClaimNames.Sub (like
                // ICurrentUserService.UserId) finds nothing and gets a false null —
                // no error, just a wrong answer. Keep claims exactly as issued.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = string.IsNullOrEmpty(signingKey)
                        ? null
                        : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.InternalStaff, p => p.RequireRole("Admin", "Manager", "Team Leader", "User"))
            .AddPolicy(PolicyNames.AdminOnly, p => p.RequireRole("Admin"))
            .AddPolicy(PolicyNames.UserManagement, p => p.RequireRole("Admin", "Manager"))
            .AddPolicy(PolicyNames.SupervisorOrAbove, p => p.RequireRole("Admin", "Manager", "Team Leader"))
            .AddPolicy(PolicyNames.AnyAuthenticatedUser, p => p.RequireAuthenticatedUser());

        return services;
    }
}
