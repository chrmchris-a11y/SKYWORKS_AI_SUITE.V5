// Phase1 Step4 — Skyworks V5
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Skyworks.AgentComm;
using Skyworks.AgentComm.Services;
using Microsoft.Extensions.FileProviders;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// HTTP Client Factory for Python Microservice
builder.Services.AddHttpClient("PythonService", client =>
{
  client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("PythonService:BaseUrl") ?? "http://localhost:8001");
  client.Timeout = TimeSpan.FromSeconds(10);
});

// Typed HttpClient for legacy API-layer PythonCalculationClient (used by SAILController)
builder.Services.Configure<Skyworks.Api.Services.PythonServiceOptions>(options =>
{
  options.BaseUrl = builder.Configuration.GetValue<string>("PythonService:BaseUrl") ?? "http://localhost:8001";
});
builder.Services.AddHttpClient<Skyworks.Api.Services.PythonCalculationClient>((sp, client) =>
{
  var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Skyworks.Api.Services.PythonServiceOptions>>().Value;
  client.BaseAddress = new Uri(opt.BaseUrl ?? "http://localhost:8001");
  client.Timeout = TimeSpan.FromSeconds(10);
});

// Localization - 13 supported languages (EN, EL, DE, FR, ES, IT, RU, ZH, PL, HR, SL, UK, CS)
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"), // English (default)
        new CultureInfo("el"), // Greek (Ελληνικά)
        new CultureInfo("de"), // German (Deutsch)
        new CultureInfo("fr"), // French (Français)
        new CultureInfo("es"), // Spanish (Español)
        new CultureInfo("it"), // Italian (Italiano)
        new CultureInfo("ru"), // Russian (Русский)
        new CultureInfo("zh"), // Chinese (中文)
        new CultureInfo("pl"), // Polish (Polski)
        new CultureInfo("hr"), // Croatian (Hrvatski)
        new CultureInfo("sl"), // Slovenian (Slovenščina)
        new CultureInfo("uk"), // Ukrainian (Українська)
        new CultureInfo("cs")  // Czech (Čeština)
    };
    
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    // Accept-Language header detection
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

// Services
builder.Services.AddControllers()
  .AddJsonOptions(options =>
  {
    // Επιτρέπουμε string τιμές για Enums (π.χ. "Medium") στα request/response bodies
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    
    // Additional JSON options for better compatibility
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
  });
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
  options.AddPolicy("DevAll", policy =>
  {
    policy
      .AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Skyworks API",
        Version = "v1",
        Description = "SKYWORKS_AI_SUITE.V5 - Minimal API surface (v1)"
    });
  // JWT Bearer in Swagger
  var securityScheme = new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header using the Bearer scheme."
  };
  c.AddSecurityDefinition("bearerAuth", securityScheme);
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    { securityScheme, new string[] { } }
  });
});

// Agent Communication System (placeholder registration)
builder.Services.AddSingleton<IAgentComm, SyncService>();

// Agent LLM Service (bridge to Python). Compute workspace root (repo root)
var contentRoot = builder.Environment.ContentRootPath; // Backend/src/Skyworks.Api
var workspaceRoot = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", ".."));
builder.Services.AddSingleton<Skyworks.Core.Services.AgentLLMService>(_ =>
  new Skyworks.Core.Services.AgentLLMService(workspaceRoot));

// Knowledge Base Service
builder.Services.AddSingleton<Skyworks.Core.Services.IKnowledgeBaseService>(_ =>
  new Skyworks.Core.Services.KnowledgeBaseService(workspaceRoot));

// GRC Calculation Service (SORA 2.5 Ground Risk Class)
builder.Services.AddSingleton<Skyworks.Core.Services.GRC.IGRCCalculationService, Skyworks.Core.Services.GRC.GRCCalculationService>();

// Population Density Provider (SORA 2.5 Table 3)
builder.Services.AddSingleton<Skyworks.Core.Services.GRC.IPopulationDensityProvider, Skyworks.Infrastructure.Services.StaticPopulationDensityProvider>();

// Airspace Provider (Step 22 – static demo polygons)
builder.Services.AddSingleton<Skyworks.Core.Services.Airspace.IAirspaceProvider, Skyworks.Infrastructure.Services.StaticAirspaceProvider>();

// Service Zones (Cyprus demo sites for cleaning services)
builder.Services.AddSingleton<Skyworks.Core.Services.ServiceZones.IServiceZoneProvider, Skyworks.Infrastructure.Services.StaticServiceZoneProvider>();

// ARC Validation (Step 23 – basic heuristic checks)
builder.Services.AddSingleton<Skyworks.Core.Services.ARC.IARCValidationService, Skyworks.Core.Services.ARC.ARCValidationService>();
// ARC Composite (Step 24)
builder.Services.AddSingleton<Skyworks.Core.Services.ARC.IARCCompositeService>(sp =>
{
    var arc = sp.GetRequiredService<Skyworks.Core.Services.GRC.IGRCCalculationService>();
    var airspace = sp.GetService<Skyworks.Core.Services.Airspace.IAirspaceProvider>();
    return new Skyworks.Core.Services.ARC.ARCCompositeService(arc, airspace);
});

// Drone Catalog Service (Step 26 prep - 61 drone models with SORA specs)
builder.Services.AddSingleton<Skyworks.Core.Services.Drones.IDroneCatalogService, Skyworks.Infrastructure.Services.Drones.StaticDroneCatalogService>();

// Streaming ARC (Step 26 – real-time)
builder.Services.AddSingleton<Skyworks.Core.Services.Streaming.IStreamingArcService, Skyworks.Core.Services.Streaming.StreamingArcService>();

// Weather Provider (Step 27 – weather data APIs)
builder.Services.AddSingleton<Skyworks.Core.Services.Weather.IWeatherProvider, Skyworks.Infrastructure.Services.Weather.StaticWeatherProvider>();

// Compliance Provider (Step 28 – appendix compliance, OSO/TMPR obligations)
builder.Services.AddSingleton<Skyworks.Core.Services.Compliance.IComplianceProvider, Skyworks.Infrastructure.Services.Compliance.StaticComplianceProvider>();

// Traffic Provider (Step 29 – air traffic density, ADS-B/OpenSky)
builder.Services.AddSingleton<Skyworks.Core.Services.Traffic.ITrafficProvider, Skyworks.Infrastructure.Services.Traffic.StaticTrafficProvider>();

// SAIL Service (Step 31 – SAIL determination JARUS SORA 2.0 Table 5 / 2.5 Table 7)
builder.Services.AddSingleton<Skyworks.Core.Services.ISAILService, Skyworks.Core.Services.SAILService>();

// OSO Service (Step 32 – OSO requirements JARUS SORA 2.0 Table 6 / 2.5 Table 14)
builder.Services.AddSingleton<Skyworks.Core.Services.IOSOService, Skyworks.Core.Services.OSOService>();

// SAIL Compliance Service (Step 33 – Consolidated compliance check)
builder.Services.AddSingleton<Skyworks.Core.Services.ISAILComplianceService>(sp =>
{
  var sail = sp.GetRequiredService<Skyworks.Core.Services.ISAILService>();
  var oso = sp.GetRequiredService<Skyworks.Core.Services.IOSOService>();
  var grc = sp.GetRequiredService<Skyworks.Core.Services.GRC.IGRCCalculationService>();
  return new Skyworks.Core.Services.SAILComplianceService(sail, oso, grc);
});

// SAIL Report Service (Step 34 – Documentation PDF generator)
builder.Services.AddSingleton<Skyworks.Core.Services.Reports.ISAILReportService>(sp =>
{
  var compliance = sp.GetRequiredService<Skyworks.Core.Services.ISAILComplianceService>();
  return new Skyworks.Core.Services.Reports.SAILReportService(compliance);
});

// Explanatory Note Verification (Phase 4 Step 38)
builder.Services.AddSingleton<Skyworks.Core.Services.Reports.IExplanatoryNoteVerificationService>(sp =>
{
  var sail = sp.GetRequiredService<Skyworks.Core.Services.ISAILService>();
  var oso = sp.GetRequiredService<Skyworks.Core.Services.IOSOService>();
  return new Skyworks.Core.Services.Reports.ExplanatoryNoteVerificationService(sail, oso);
});

// Risk Modeling (Step 35 – heuristic advisory scoring)
builder.Services.AddSingleton<Skyworks.Core.Services.Risk.IRiskModelingService, Skyworks.Core.Services.Risk.RiskModelingService>();

// Historical Analysis (Step 36 – trends & statistics across SORA 2.0/2.5)
builder.Services.AddSingleton<Skyworks.Core.Services.History.IHistoricalAnalysisService, Skyworks.Core.Services.History.HistoricalAnalysisService>();

// SORA Orchestration (Step 37 – unified E2E pipeline)
builder.Services.AddSingleton<Skyworks.Core.Services.Orchestration.ISORAOrchestrationService>(sp =>
{
  var grc = sp.GetRequiredService<Skyworks.Core.Services.GRC.IGRCCalculationService>();
  var sail = sp.GetRequiredService<Skyworks.Core.Services.ISAILService>();
  var oso = sp.GetRequiredService<Skyworks.Core.Services.IOSOService>();
  var risk = sp.GetRequiredService<Skyworks.Core.Services.Risk.IRiskModelingService>();
  var arcValidation = sp.GetService<Skyworks.Core.Services.ARC.IARCValidationService>();
  var py = sp.GetService<Skyworks.Core.Services.Python.IPythonCalculationClient>();
  var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<Skyworks.Core.Services.Orchestration.SORAOrchestrationService>>();
  // Read proxy-only setting from configuration
  var configuration = sp.GetRequiredService<IConfiguration>();
  var proxyOnly = configuration.GetValue<bool>("SORA:ProxyOnly");
  var allowExplicitARC = configuration.GetValue<bool>("SORA:AllowExplicitARC");
  return new Skyworks.Core.Services.Orchestration.SORAOrchestrationService(grc, sail, oso, risk, logger, arcValidation, null, py, proxyOnly, allowExplicitARC);
});

// GIS Service (Phase 6 Step 51 – Operational volumes, population density, airspace, no-fly zones, terrain)
builder.Services.AddSingleton<Skyworks.Core.Services.GIS.IGISService, Skyworks.Core.Services.GIS.GISService>();

// Python Calculation Client (100% EASA/JARUS accurate calculations)
builder.Services.AddSingleton<Skyworks.Core.Services.Python.IPythonCalculationClient, Skyworks.Core.Services.Python.PythonCalculationClient>();

// Airports Declared Distances (TORA/TODA/ASDA/LDA) - static sample provider
builder.Services.AddSingleton<Skyworks.Core.Services.Airports.IAirportDeclaredDistancesProvider, Skyworks.Infrastructure.Services.Airports.StaticAirportDeclaredDistancesProvider>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev_secret_key_change_me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Skyworks";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SkyworksUsers";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = key
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Request Localization Middleware (must be early in pipeline)
app.UseRequestLocalization();

// Auto-train Knowledge Base index on startup (non-blocking)
app.Lifetime.ApplicationStarted.Register(() =>
{
  _ = Task.Run(async () =>
  {
    try
    {
      using var scope = app.Services.CreateScope();
      var kb = scope.ServiceProvider.GetService<Skyworks.Core.Services.IKnowledgeBaseService>();
      if (kb != null)
      {
        await kb.TrainAsync();
      }
    }
    catch
    {
      // best-effort
    }
  });
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skyworks API v1");
});

// Auth
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("DevAll");

// Serve Frontend from /app (Frontend/ root directory)
var frontendRoot = Path.Combine(workspaceRoot, "Frontend");
if (Directory.Exists(frontendRoot))
{
  var fp = new PhysicalFileProvider(frontendRoot);
  app.UseDefaultFiles(new DefaultFilesOptions
  {
    RequestPath = "/app",
    FileProvider = fp,
    DefaultFileNames = new[] { "index.html" }
  });
  app.UseStaticFiles(new StaticFileOptions
  {
    RequestPath = "/app",
    FileProvider = fp
  });

  // Map favicon to an existing PNG in Frontend/assets to remove 404 noise
  var assetsPath = Path.Combine(frontendRoot, "assets", "logo drone skyworks.png");
  if (File.Exists(assetsPath))
  {
    app.MapGet("/favicon.ico", () => Results.File(assetsPath, contentType: "image/png"));
  }
}

// Root redirect to /app (simpler UX) - must be before MapControllers
app.MapGet("/", () => Results.Redirect("/app/", permanent: false));

// SignalR Hubs
app.MapHub<Skyworks.Api.Hubs.StreamingHub>("/hubs/arc");

// Enable controllers
app.MapControllers();

// ---- Minimal API (v1) ----
var v1 = app.MapGroup("/api/v1").WithTags("v1");

// Health (Primary endpoint at root /health)
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Skyworks Backend API",
    port = 5210,
    python_api = "http://localhost:8001",
    timestamp = DateTime.UtcNow
}))
  .WithName("HealthCheckRoot")
  .WithSummary("Health probe (root)")
  .WithDescription("Returns the service liveness status at /health.");

// Python backend health check
app.MapGet("/health/python", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("PythonService");
        var response = await client.GetAsync("/");
        return Results.Ok(new
        {
            status = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
            python_backend = "http://localhost:8001",
            statusCode = (int)response.StatusCode,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "unhealthy",
            python_backend = "http://localhost:8001",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
})
  .WithName("PythonHealthCheck")
  .WithSummary("Python backend health")
  .WithDescription("Checks connectivity to Python backend at port 8001.");

// Health (v1 legacy endpoint)
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

// Required for WebApplicationFactory integration testing
public partial class Program { }
