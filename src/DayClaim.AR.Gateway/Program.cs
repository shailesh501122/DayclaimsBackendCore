using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ocelot.json's DownstreamHostAndPorts default ("ar-api":8080) only resolves
// inside the local Docker Compose network. On a platform where the AR API
// is a separate, independently-addressed service (e.g. Render), override it
// here from config/env vars — added as a later provider so it wins over the
// json file for the same keys. All three routes proxy to the same API, so
// one override covers all of them.
var arApiHost = builder.Configuration["ArApi:Host"];
if (!string.IsNullOrEmpty(arApiHost))
{
    var arApiPort = builder.Configuration["ArApi:Port"] ?? "443";
    var arApiScheme = builder.Configuration["ArApi:Scheme"] ?? "https";
    var routeCount = builder.Configuration.GetSection("Routes").GetChildren().Count();

    var overrides = new Dictionary<string, string?>();
    for (var i = 0; i < routeCount; i++)
    {
        overrides[$"Routes:{i}:DownstreamHostAndPorts:0:Host"] = arApiHost;
        overrides[$"Routes:{i}:DownstreamHostAndPorts:0:Port"] = arApiPort;
        overrides[$"Routes:{i}:DownstreamScheme"] = arApiScheme;
    }

    builder.Configuration.AddInMemoryCollection(overrides);
}

// Same JWT signing key/issuer as the AR API — the gateway validates the
// token before proxying, matching the deck's "ALB -> Ocelot API Gateway:
// authentication enforcement (JWT), routing, rate-limiting, throttling".
var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"] ?? string.Empty;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = string.IsNullOrEmpty(signingKey) ? null : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddOcelot(builder.Configuration).AddPolly();

var app = builder.Build();

app.UseAuthentication();
await app.UseOcelot();

app.Run();
