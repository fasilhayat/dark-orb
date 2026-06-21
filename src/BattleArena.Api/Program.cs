using BattleArena.Api;
using BattleArena.Api.Endpoints;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "BattleArena API", Version = "v1" });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter your API key"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("ApiKey", doc, null),
            new List<string>()
        }
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

var app = builder.Build();

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    ctx.Response.StatusCode  = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync("{\"error\":\"An unexpected error occurred.\"}");
}));

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("LocalDev"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BattleArena API v1");
    });
}

var apiKeyOptions = app.Services.GetRequiredService<IOptions<ApiKeyOptions>>().Value;

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/swagger") &&
        !context.Request.Path.StartsWithSegments("/api/healthcheck"))
    {
        if (!string.IsNullOrEmpty(apiKeyOptions.BattleArena))
        {
            context.Request.Headers.TryGetValue("X-Api-Key", out var key);
            var provided = Encoding.UTF8.GetBytes((string?)key ?? string.Empty);
            var expected = Encoding.UTF8.GetBytes(apiKeyOptions.BattleArena);
            if (!CryptographicOperations.FixedTimeEquals(provided, expected))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: missing or invalid X-Api-Key header.");
                return;
            }
        }
    }
    await next(context);
});

app.MapHealthEndpoints();
app.MapCharacterEndpoints();
app.MapEquipmentEndpoints();
app.MapAccessoriesEndpoints();
app.MapNpcEndpoints();
app.MapLoreEndpoints();
app.MapQuestEndpoints();

app.Run();
