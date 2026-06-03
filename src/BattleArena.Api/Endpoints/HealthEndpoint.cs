using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace BattleArena.Api.Endpoints;

public static class HealthEndpoint
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/api/healthcheck", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.ToString(),
                    entries = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.ToString(),
                        description = e.Value.Description
                    })
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        }).ExcludeFromDescription();
    }
}
