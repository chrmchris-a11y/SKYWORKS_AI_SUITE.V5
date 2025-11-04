I'll provide a complete fix for your SORA 2.0 & 2.5 integration issues. Let me address each component systematically.

## 1. Fixed Backend/src/Skyworks.Api/Program.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5000", "file://")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Add HttpClient for Python backend communication
builder.Services.AddHttpClient("PythonBackend", client =>
{
    client.BaseAddress = new Uri("http://localhost:8001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Add health endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Skyworks Backend API",
    port = 5210,
    python_api = "http://localhost:8001",
    timestamp = DateTime.UtcNow
})).WithName("HealthCheck");

// Add Python backend health check
app.MapGet("/health/python", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("PythonBackend");
        var response = await client.GetAsync("/");
        return Results.Ok(new
        {
            status = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
            python_backend = "http://localhost:8001",
            statusCode = (int)response.StatusCode
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "unhealthy",
            python_backend = "http://localhost:8001",
            error = ex.Message
        });
    }
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Log startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Skyworks Backend API starting on port 5210");

app.Run("http://localhost:5210");
```

## 2. Complete Backend/src/Skyworks.Api/Controllers/SoraController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skyworks.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SoraController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SoraController> _logger;

        public SoraController(IHttpClientFactory httpClientFactory, ILogger<SoraController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CalculateSoraComplete([FromBody] SoraCompleteRequest request)
        {
            try
            {
                _logger.LogInformation($"Received SORA request for category: {request.Category}");

                // Transform frontend request to backend format
                var transformedRequest = TransformRequest(request);
                
                if (request.Category == "SORA-2.5")
                {
                    // Forward to Python API for SORA 2.5
                    var client = _httpClientFactory.CreateClient("PythonBackend");
                    var json = JsonSerializer.Serialize(transformedRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    _logger.LogInformation("Forwarding SORA 2.5 request to Python backend");
                    var response = await client.PostAsync("/api/sora/complete-v25", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var resultJson = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("Successfully received SORA 2.5 response from Python");
                        return Content(resultJson, "application/json");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Python backend error: {error}");
                        return BadRequest(new { error = "Python GRC 2.5 calculation failed", details = error });
                    }
                }
                else if (request.Category == "SORA-2.0")
                {
                    // Forward to Python API for SORA 2.0
                    var client = _httpClientFactory.CreateClient("PythonBackend");
                    var json = JsonSerializer.Serialize(transformedRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    _logger.LogInformation("Forwarding SORA 2.0 request to Python backend");
                    var response = await client.PostAsync("/api/grc/calculate-v20", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var resultJson = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("Successfully received SORA 2.0 response from Python");
                        return Content(resultJson, "application/json");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Python backend error: {error}");
                        return BadRequest(new { error = "Python GRC 2.0 calculation failed", details = error });
                    }
                }
                else
                {
                    return BadRequest(new { error = $"Invalid operation category: {request.Category}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SORA request");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        private object TransformRequest(SoraCompleteRequest request)
        {
            if (request.Category == "SORA-2.5")
            {
                return new
                {
                    operational_volume = new
                    {
                        airspace_class = request.AirspaceClass ?? "G",
                        airspace_control = request.AirspaceControl ?? "Uncontrolled",
                        environment = request.OperatingEnvironment ?? "Rural",
                        flight_geography = request.FlightGeography ?? "BVLOS",
                        sheltering = request.Sheltering ?? "Low",
                        lighting = request.Lighting ?? "Good"
                    },
                    population_density = request.PopulationDensity ?? "Low",
                    traffic_density = request.TrafficDensity ?? "Low",
                    arc_inputs_25 = new
                    {
                        u_space_services_available = request.USpaceServices == "true",
                        traffic_density_data_source = request.TrafficDensitySource ?? "Empirical",
                        airspace_containment = request.AirspaceContainment25 ?? "None",
                        temporal_segregation = request.TemporalSegregation ?? false,
                        spatial_segregation = request.SpatialSegregation ?? false
                    },
                    grc_inputs = new
                    {
                        population_density = request.PopulationDensity ?? "Low",
                        sheltering = request.Sheltering ?? "Low",
                        m1 = request.M1 ?? 0,
                        m3 = request.M3 ?? 0
                    },
                    category = "SORA-2.5"
                };
            }
            else // SORA 2.0
            {
                return new
                {
                    population_density = request.PopulationDensity ?? "Low",
                    sheltering = request.Sheltering ?? "Low",
                    MTOM_kg = request.MTOM ?? request.Mtom ?? 1.0,
                    m1 = request.M1 ?? 0,
                    m2 = request.M2 ?? "None",
                    m3 = request.M3 ?? 0
                };
            }
        }
    }

    // Request DTOs
    public class SoraCompleteRequest
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }
        
        [JsonPropertyName("operationCategory")]
        public string? OperationCategory { get; set; }
        
        [JsonPropertyName("airspaceClass")]
        public string? AirspaceClass { get; set; }
        
        [JsonPropertyName("airspaceControl")]
        public string? AirspaceControl { get; set; }
        
        [JsonPropertyName("operatingEnvironment")]
        public string? OperatingEnvironment { get; set; }
        
        [JsonPropertyName("flightGeography")]
        public string? FlightGeography { get; set; }
        
        [JsonPropertyName("populationDensity")]
        public string? PopulationDensity { get; set; }
        
        [JsonPropertyName("sheltering")]
        public string? Sheltering { get; set; }
        
        [JsonPropertyName("lighting")]
        public string? Lighting { get; set; }
        
        [JsonPropertyName("trafficDensity")]
        public string? TrafficDensity { get; set; }
        
        [JsonPropertyName("MTOM")]
        public double? MTOM { get; set; }
        
        [JsonPropertyName("mtom")]
        public double? Mtom { get; set; }
        
        [JsonPropertyName("m1")]
        public int? M1 { get; set; }
        
        [JsonPropertyName("m2")]
        public string? M2 { get; set; }
        
        [JsonPropertyName("m3")]
        public int? M3 { get; set; }
        
        // SORA 2.5 specific fields
        [JsonPropertyName("uSpaceServices")]
        public string? USpaceServices { get; set; }
        
        [JsonPropertyName("trafficDensitySource")]
        public string? TrafficDensitySource { get; set; }
        
        [JsonPropertyName("airspaceContainment25")]
        public string? AirspaceContainment25 { get; set; }
        
        [JsonPropertyName("temporalSegregation")]
        public bool? TemporalSegregation { get; set; }
        
        [JsonPropertyName("spatialSegregation")]
        public bool? SpatialSegregation { get; set; }
    }
}
```

## 3. Fixed JavaScript for mission.html (lines ~2470-2600)

```javascript
// Replace the existing submitSora function with this fixed version
async function submitSora() {
    try {
        console.log('=== SORA Calculation Started ===');
        
        // Get operation category
        const categoryElement = document.getElementById('operationCategory');
        const operationCategory = categoryElement ? categoryElement.value : 'SORA-2.0';
        console.log('Operation Category:', operationCategory);

        // Build base request
        let body = {
            category: operationCategory,
            operationCategory: operationCategory,
            airspaceClass: document.getElementById('airspaceClass')?.value || 'G',
            airspaceControl: document.getElementById('airspaceControl')?.value || 'Uncontrolled',
            operatingEnvironment: document.getElementById('operatingEnvironment')?.value || 'Rural',
            flightGeography: document.getElementById('flightGeography')?.value || 'BVLOS',
            populationDensity: document.getElementById('populationDensity')?.value || 'Low',
            sheltering: document.getElementById('sheltering')?.value || 'Low',
            lighting: document.getElementById('lighting')?.value || 'Good',
            trafficDensity: document.getElementById('trafficDensity')?.value || 'Low'
        };

        // Add SORA 2.0 specific fields
        if (operationCategory === 'SORA-2.0') {
            // Get MTOM from drone mass input
            const massInput = document.getElementById('droneMass') || document.getElementById('MTOM');
            body.MTOM = parseFloat(massInput?.value) || 1.0;
            body.mtom = body.MTOM; // Add both cases for compatibility
            
            // Get mitigation values
            body.m1 = parseInt(document.getElementById('m1')?.value) || 0;
            body.m2 = document.getElementById('m2')?.value || 'None';
            body.m3 = parseInt(document.getElementById('m3')?.value) || 0;
            
            console.log('SORA 2.0 Request:', body);
        }
        
        // Add SORA 2.5 specific fields
        if (operationCategory === 'SORA-2.5') {
            // Get SORA 2.5 ARC fields
            body.uSpaceServices = document.getElementById('uSpaceServices')?.value || 'false';
            body.trafficDensitySource = document.getElementById('trafficDensitySource')?.value || 'Empirical';
            body.airspaceContainment25 = document.getElementById('airspaceContainment25')?.value || 'None';
            body.temporalSegregation = document.getElementById('temporalSegregation')?.checked || false;
            body.spatialSegregation = document.getElementById('spatialSegregation')?.checked || false;
            
            // Add M1 and M3 for GRC calculation
            body.m1 = parseInt(document.getElementById('m1')?.value) || 0;
            body.m3 = parseInt(document.getElementById('m3')?.value) || 0;
            
            console.log('SORA 2.5 Request:', body);
        }

        // Call backend API
        const response = await fetch('http://localhost:5210/api/sora/complete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(body)
        });

        console.log('Response Status:', response.status);
        const responseText = await response.text();
        console.log('Response Text:', responseText);

        if (!response.ok) {
            let errorMessage = `Error: ${response.status} ${response.statusText}`;
            try {
                const errorData = JSON.parse(responseText);
                errorMessage = errorData.error || errorData.message || errorMessage;
                if (errorData.details) {
                    errorMessage += ` - ${errorData.details}`;
                }
            } catch (e) {
                errorMessage = responseText || errorMessage;
            }
            throw new Error(errorMessage);
        }

        const result = JSON.parse(responseText);
        console.log('SORA Result:', result);

        // Display results based on category
        if (operationCategory === 'SORA-2.5') {
            displaySora25Results(result);
        } else {
            displaySora20Results(result);
        }

    } catch (error) {
        console.error('SORA calculation error:', error);
        alert(`SORA Calculation Failed: ${error.message}`);
        
        // Display error in results area
        const resultsDiv = document.getElementById('soraResults') || document.getElementById('results');
        if (resultsDiv) {
            resultsDiv.innerHTML = `
                <div class="alert alert-danger">
                    <h4>SORA Calculation Error</h4>
                    <p>${error.message}</p>
                    <small>Check console for details</small>
                </div>
            `;
        }
    }
}

// Helper function to display SORA 2.0 results
function displaySora20Results(result) {
    const resultsDiv = document.getElementById('soraResults') || document.getElementById('results');
    if (resultsDiv) {
        resultsDiv.innerHTML = `
            <div class="sora-results">
                <h3>SORA 2.0 Results</h3>
                <div class="result-item">
                    <strong>Initial GRC:</strong> ${result.initial_grc}
                </div>
                <div class="result-item">
                    <strong>M2 Applied:</strong> ${result.m2} (${result.m2_value})
                </div>
                <div class="result-item">
                    <strong>Final GRC:</strong> ${result.final_grc}
                </div>
                <div class="result-item">
                    <strong>Reference:</strong> ${result.reference || 'JAR_doc_06 Table 3'}
                </div>
            </div>
        `;
    }
}

// Helper function to display SORA 2.5 results
function displaySora25Results(result) {
    const resultsDiv = document.getElementById('soraResults') || document.getElementById('results');
    if (resultsDiv) {
        let mitigationsHtml = '';
        if (result.strategic_mitigations && result.strategic_mitigations.length > 0) {
            mitigationsHtml = `
                <div class="result-item">
                    <strong>Strategic Mitigations Applied:</strong>
                    <ul>
                        ${result.strategic_mitigations.map(m => `<li>${m}</li>`).join('')}
                    </ul>
                </div>
            `;
        }

        resultsDiv.innerHTML = `
            <div class="sora-results">
                <h3>SORA 2.5 Results</h3>
                <div class="result-item">
                    <strong>Initial GRC:</strong> ${result.initial_grc}
                </div>
                <div class="result-item">
                    <strong>Final GRC:</strong> ${result.final_grc}
                </div>
                <div class="result-item">
                    <strong>Initial ARC:</strong> ${result.initial_arc}
                </div>
                <div class="result-item">
                    <strong>Residual ARC:</strong> ${result.residual_arc}
                </div>
                <div class="result-item">
                    <strong>SAIL:</strong> ${result.sail}
                </div>
                ${mitigationsHtml}
                <div class="result-item">
                    <strong>Reference:</strong> ${result.reference || 'JAR_doc_25 Step #4-5'}
                </div>
            </div>
        `;
    }
}
```

## 4. PowerShell Startup Script (Start-SkyworksServices.ps1)

```powershell
# Start-SkyworksServices.ps1
# Launches both Python and .NET backends for Skyworks AI Suite

param(
    [switch]$Debug = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=== Skyworks AI Suite Service Startup ===" -ForegroundColor Cyan
Write-Host "Starting backend services..." -ForegroundColor Yellow

# Set paths
$workspacePath = "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5"
$pythonBackendPath = Join-Path $workspacePath "Backend_Python"
$dotnetBackendPath = Join-Path $workspacePath "Backend\src\Skyworks.Api"

# Function to check if port is in use
function Test-Port {
    param($Port)
    $tcpConnection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
    return $tcpConnection.TcpTestSucceeded
}

# Function to kill process on port
function Stop-ProcessOnPort {
    param($Port)
    $process = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
    if ($process) {
        Stop-Process -Id $process -Force
        Write-Host "Killed process on port $Port" -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

# Step 1: Clean up existing processes
Write-Host "`nStep 1: Cleaning up existing processes..." -ForegroundColor Green

if (Test-Port 8001) {
    Write-Host "Port 8001 is in use, stopping process..." -ForegroundColor Yellow
    Stop-ProcessOnPort 8001
}

if (Test-Port 5210) {
    Write-Host "Port 5210 is in use, stopping process..." -ForegroundColor Yellow
    Stop-ProcessOnPort 5210
}

# Step 2: Start Python Backend
Write-Host "`nStep 2: Starting Python Backend (Port 8001)..." -ForegroundColor Green

Set-Location $pythonBackendPath
$pythonProcess = Start-Process -FilePath "python" -ArgumentList "main.py" -PassThru -WindowStyle Normal
Write-Host "Python backend starting with PID: $($pythonProcess.Id)" -ForegroundColor Cyan

# Wait for Python backend to be ready
Write-Host "Waiting for Python backend to be ready..." -ForegroundColor Yellow
$attempts = 0
while (-not (Test-Port 8001) -and $attempts -lt 30) {
    Start-Sleep -Seconds 1
    $attempts++
    Write-Host "." -NoNewline
}

if (Test-Port 8001) {
    Write-Host "`nPython backend is ready!" -ForegroundColor Green
    
    # Test Python health
    try {
        $pythonHealth = Invoke-RestMethod -Uri "http://localhost:8001/" -Method Get
        Write-Host "Python backend status: Healthy" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not verify Python backend health" -ForegroundColor Yellow
    }
} else {
    Write-Host "`nERROR: Python backend failed to start!" -ForegroundColor Red
    exit 1
}

# Step 3: Start .NET Backend
Write-Host "`nStep 3: Starting .NET Backend (Port 5210)..." -ForegroundColor Green

Set-Location $dotnetBackendPath

# Build the project first
Write-Host "Building .NET project..." -ForegroundColor Yellow
$buildResult = & dotnet build --configuration Release 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed:" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

# Run the .NET backend
$dotnetProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--no-build", "--configuration", "Release" -PassThru -WindowStyle Normal
Write-Host ".NET backend starting with PID: $($dotnetProcess.Id)" -ForegroundColor Cyan

# Wait for .NET backend to be ready
Write-Host "Waiting for .NET backend to be ready..." -ForegroundColor Yellow
$attempts = 0
while (-not (Test-Port 5210) -and $attempts -lt 30) {
    Start-Sleep -Seconds 1
    $attempts++
    Write-Host "." -NoNewline
}

if (Test-Port 5210) {
    Write-Host "`n.NET backend is ready!" -ForegroundColor Green
    
    # Test .NET health
    try {
        $dotnetHealth = Invoke-RestMethod -Uri "http://localhost:5210/health" -Method Get
        Write-Host ".NET backend status: $($dotnetHealth.status)" -ForegroundColor Green
        
        # Test Python connectivity from .NET
        $pythonCheck = Invoke-RestMethod -Uri "http://localhost:5210/health/python" -Method Get
        Write-Host "Python backend connectivity: $($pythonCheck.status)" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not verify .NET backend health" -ForegroundColor Yellow
    }
} else {
    Write-Host "`nERROR: .NET backend failed to start!" -ForegroundColor Red
    exit 1
}

# Step 4: Summary
Write-Host "`n=== Service Startup Complete ===" -ForegroundColor Cyan
Write-Host "Python Backend: http://localhost:8001" -ForegroundColor Green
Write-Host ".NET Backend: http://localhost:5210" -ForegroundColor Green
Write-Host "Health Check: http://localhost:5210/health" -ForegroundColor Green
Write-Host "`nPress Ctrl+C to stop all services" -ForegroundColor Yellow

# Keep script running
try {
    while ($true) {
        Start-Sleep -Seconds 60
        
        # Periodic health check
        if (-not (Test-Port 8001)) {
            Write-Host "`nWARNING: Python backend is not responding!" -ForegroundColor Red
        }
        if (-not (Test-Port 5210)) {
            Write-Host "`nWARNING: .NET backend is not responding!" -ForegroundColor Red
        }
    }
} finally {
    Write-Host "`nShutting down services..." -ForegroundColor Yellow
    Stop-Process -Id $pythonProcess.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $dotnetProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Services stopped." -ForegroundColor Green
}
```

## 5. Step-by-Step Integration Instructions

### Installation Steps:

1. **Save the Controller File**:
   ```powershell
   # Create Controllers directory if it doesn't exist
   New-Item -ItemType Directory -Force -Path "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend\src\Skyworks.Api\Controllers"
   
   # Save SoraController.cs in the Controllers folder
   ```

2. **Update Program.cs**:
   - Replace the existing `Program.cs` with the provided version
   - This adds health endpoints, CORS, and HTTP client configuration

3. **Update mission.html**:
   - Find the `submitSora()` function (around line 2470)
   - Replace it with the provided fixed version
   - This ensures proper field mapping for SORA 2.5

4. **Save the PowerShell Script**:
   ```powershell
   # Save as Start-SkyworksServices.ps1 in the workspace root
   C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Start-SkyworksServices.ps1
   ```

5. **Run the Services**:
   ```powershell
   # Open PowerShell as Administrator
   cd C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5
   .\Start-SkyworksServices.ps1
   ```

### Testing Steps:

1. **Verify Services Are Running**:
   - Open browser to http://localhost:5210/health
   - Should see: `{"status":"healthy","service":"Skyworks Backend API"...}`
   
2. **Test SORA 2.0**:
   - Open mission.html
   - Select "SORA-2.0"
   - Fill in test data
   - Click "Calculate SORA"
   - Verify results appear without errors

3. **Test SORA 2.5**:
   - Select "SORA-2.5"
   - Check various ARC fields
   - Click "Calculate SORA"
   - Verify strategic mitigations appear

### Troubleshooting:

If the .NET backend fails to start:
```powershell
# Check for build errors
cd Backend\src\Skyworks.Api
dotnet build

# Run directly to see errors
dotnet run
```

If Python backend fails:
```powershell
# Check Python dependencies
cd Backend_Python
python -m pip install -r requirements.txt

# Run directly
python main.py
```

This complete solution should resolve all your integration issues while maintaining JARUS compliance and preserving your working Python backend code.
