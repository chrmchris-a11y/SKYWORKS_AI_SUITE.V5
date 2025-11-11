// Phase1 Step4 — Skyworks V5
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Skyworks.AgentComm;
using Skyworks.AgentComm.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Skyworks API",
        Version = "v1",
        Description = "SKYWORKS_AI_SUITE.V5 - Minimal API surface (v1)"
    });
});

// Agent Communication System (placeholder registration)
builder.Services.AddSingleton<IAgentComm, SyncService>();

// Agent LLM Service (bridge to Python). Compute workspace root (repo root)
var contentRoot = builder.Environment.ContentRootPath; // Backend/src/Skyworks.Api
var workspaceRoot = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", ".."));
builder.Services.AddSingleton<Skyworks.Core.Services.AgentLLMService>(_ =>
  new Skyworks.Core.Services.AgentLLMService(workspaceRoot));

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skyworks API v1");
});

// Static files (serve UI from WebPlatform/wwwroot)
var webPlatformRoot = Path.Combine(workspaceRoot, "WebPlatform", "wwwroot");
if (Directory.Exists(webPlatformRoot))
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".js"] = "application/javascript; charset=utf-8";
    
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(webPlatformRoot),
        RequestPath = "",
        ContentTypeProvider = provider
    });
}

// Enable controllers
app.MapControllers();

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

// Agent Communication Ping (stub)
v1.MapGet("/agent/ping", async (IAgentComm agentComm) =>
{
    var response = await agentComm.PingAsync();
    return Results.Ok(new { message = response, timestamp = DateTime.UtcNow });
})
  .WithName("AgentCommPing")
  .WithSummary("Agent communication ping")
  .WithDescription("Pings the agent communication system to verify connectivity (stub).");

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }