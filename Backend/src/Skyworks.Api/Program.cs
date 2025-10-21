// Phase1 Step3 — Skyworks V5
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Skyworks API",
        Version = "v1",
        Description = "SKYWORKS_AI_SUITE.V5 — Minimal API surface (v1)"
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skyworks API v1");
});

// ---- Minimal API (v1) ----
var v1 = app.MapGroup("/api/v1").WithTags("v1");

// Health
v1.MapGet("/health", () => Results.Ok(new { status = "OK" }))
  .WithName("HealthCheck")
  .WithSummary("Health probe")
  .WithDescription("Returns the service liveness status (v1).");

// Info
v1.MapGet("/info", () => Results.Ok(new
{
    service = "Skyworks API",
    version = "v1",
    timeUtc = DateTime.UtcNow
}))
  .WithName("ServiceInfo")
  .WithSummary("Service info")
  .WithDescription("Returns basic service metadata (v1).");

app.Run();
