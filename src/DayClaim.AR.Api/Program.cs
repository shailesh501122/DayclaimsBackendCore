using System.Threading.RateLimiting;
using DayClaim.AR.Api.Middleware;
using DayClaim.AR.Application;
using DayClaim.AR.Infrastructure;
using DayClaim.AR.Infrastructure.Persistence;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// CloudWatch is the centralized-logging target in production (deck slide 18);
// structured console logging here is what the container log driver ships there.
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers()
    // Enums as their name ("Global", not 1) — a plain integer forces every
    // API consumer to hardcode the enum's numeric ordering just to read a
    // response, and silently breaks if the enum is ever reordered.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DayClaim AR API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT access token issued by POST /api/v1/auth/login",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Global per-client-IP throttle — Ocelot applies a second layer at the gateway (deck slide 15).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

// "ready" checks hit Postgres/Redis and are for real monitoring — they must
// NOT gate the platform's own startup health probe, or a not-yet-provisioned
// dependency (e.g. Redis) would make the platform kill an otherwise-fine deploy.
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres") ?? string.Empty, name: "postgres", tags: ["ready"])
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis", tags: ["ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// /health/live has no dependency checks — it just confirms Kestrel is up —
// and is what the platform's own startup probe should hit. /health runs the
// "ready" (Postgres/Redis) checks for real uptime monitoring, and is allowed
// to report 503 without that killing the deploy.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

// Schema migration is safe to run unconditionally — it's just DDL, no
// credentials involved (see docs/ARCHITECTURE.md §6 re: a real deployment
// should eventually do this as a separate release step instead). It runs
// AFTER app.Run() below has bound Kestrel to its port — this must never be
// awaited inline before app.Run(), or a slow/unreachable database delays
// port binding past the platform's health-check timeout and fails the
// deploy even though the app itself is fine.
var migrationTask = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // A freshly-created database and a freshly-created service can take a few
    // extra seconds to become network-reachable to each other — retry with
    // backoff instead of letting one transient connection failure crash the
    // whole process on startup.
    const int maxAttempts = 5;
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            startupLogger.LogWarning(ex, "Migration attempt {Attempt}/{MaxAttempts} failed, retrying in {DelaySeconds}s", attempt, maxAttempts, delay.TotalSeconds);
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "Migration failed after {MaxAttempts} attempts", maxAttempts);
            return;
        }
    }

    // Demo-data seeding creates a well-known admin/admin account (see
    // DevSeeder) — that must be an explicit, deliberate opt-in, never
    // implied by environment name alone. Set SeedDemoData=true only for a
    // throwaway/demo deployment, never one holding real client data.
    if (builder.Configuration.GetValue("SeedDemoData", false))
    {
        await DayClaim.AR.Infrastructure.Persistence.Seed.DevSeeder.SeedAsync(scope.ServiceProvider);
    }

    // EF Core compiles a query's expression tree into a reusable plan the
    // first time it runs — on a memory-constrained container this first
    // compilation can take tens of seconds, and without this warm-up the
    // very first real user to log in after a (re)start is the one who pays
    // that cost, sometimes past a reverse proxy's timeout. Run the exact
    // query shape LoginCommand uses here, in the background, so nobody's
    // login request has to eat it.
    try
    {
        await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOrganizations)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == "__warmup__" && !u.IsDeleted);
    }
    catch (Exception ex)
    {
        startupLogger.LogWarning(ex, "EF Core query warm-up failed (non-fatal)");
    }
});

var runTask = app.RunAsync();
await Task.WhenAll(migrationTask, runTask);

public partial class Program
{
}
