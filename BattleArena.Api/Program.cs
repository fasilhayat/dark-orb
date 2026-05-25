using BattleArena.Api;
using BattleArena.Api.Endpoints;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "BattleArena API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BattleArena API v1");
});

app.MapCombatEndpoints();
app.MapCharacterEndpoints();
app.MapEquipmentEndpoints();
app.MapAccessoriesEndpoints();
app.MapNpcEndpoints();

app.Run();
