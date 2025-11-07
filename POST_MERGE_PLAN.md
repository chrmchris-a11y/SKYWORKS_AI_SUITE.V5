# 🚀 Post-Merge Plan - Phase 6 Airspace Maps
## PR: feat/ui-mission-planner-spec-pack → main

**Squash & Merge Strategy**: Combine 3 commits (b278cc1, 3971c9c, ac8c783) into single commit on main

---

## Step 1: Create Git Tag (vX.Y.1) ✅

**After Merge**:
```powershell
# Pull merged main
git checkout main
git pull origin main

# Create annotated tag
git tag -a v0.6.1 -m "feat(ui): Mission Planner UI + Phase 6 Airspace Maps

- 12-page Mission Planner UI (Annex B/AMC1 aligned)
- Phase 6 Airspace Maps (2D/3D, routes, EU layers, SORA compliance)
- Import/export GeoJSON/KML/CSV
- EU airspace layers (14 toggles)
- SORA badges (iGRC/fGRC/iARC/rARC/SAIL)
- 18 e2e tests (ban-tokens enforced)
- ZIP packaging (27.38 KB, 17 files)

Tests: 19/19 unit+golden, 18/18 e2e Playwright
Compliance: EU-only terminology, no ACE, no Mode-S veil"

# Push tag to GitHub
git push origin v0.6.1

# Verify tag
git tag -l -n9 v0.6.1
```

**Expected Output**:
```
v0.6.1          feat(ui): Mission Planner UI + Phase 6 Airspace Maps
                - 12-page Mission Planner UI (Annex B/AMC1 aligned)
                - Phase 6 Airspace Maps (2D/3D, routes, EU layers, SORA compliance)
                ...
```

---

## Step 2: Staging Deploy ✅

### 2.1 Build & Package

```powershell
# Build backend (dotnet)
cd Backend
dotnet build Skyworks.sln --configuration Release

# Build UI package
cd ..
.\scripts\pack-ui.ps1

# Verify ZIP artifact
Test-Path dist\skyworks_static_ui.zip  # Should return True
```

### 2.2 Deploy to Staging Environment

**Option A: Azure App Service**
```powershell
# Publish backend
cd Backend
dotnet publish src/Skyworks.Api/Skyworks.Api.csproj `
  --configuration Release `
  --output ../publish

# Deploy to Azure (using Azure CLI)
az webapp deployment source config-zip `
  --resource-group rg-skyworks-staging `
  --name app-skyworks-staging `
  --src ../publish.zip
```

**Option B: Docker Container**
```dockerfile
# Dockerfile (create if needed)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY Backend/publish .
COPY WebPlatform/wwwroot ./wwwroot
ENV ASPNETCORE_URLS=http://+:5210
EXPOSE 5210
ENTRYPOINT ["dotnet", "Skyworks.Api.dll"]
```

```powershell
# Build & run
docker build -t skyworks:v0.6.1 .
docker run -p 5210:5210 -e CESIUM_ION_TOKEN=$env:CESIUM_ION_TOKEN skyworks:v0.6.1
```

**Option C: Local Staging (for testing)**
```powershell
# Start backend
cd Backend
$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://0.0.0.0:5210

# Start Python FastAPI (in new terminal)
cd Backend_Python
.\venv\Scripts\python.exe -m uvicorn main:app --host 0.0.0.0 --port 8001
```

### 2.3 Run Smoke Tests (7 steps)

Execute **SMOKE_TESTS.md** checklist:
1. Import GeoJSON → verify route + CGA
2. Toggle 2D/3D → verify no errors
3. Layers → enable RMZ/CTR/UAS Geo Zones
4. Draw → waypoint/geofence/CGA
5. SORA POST → badges update
6. Export → GeoJSON/KML/CSV download
7. Console → logs visible

**Pass Criteria**: All 7 tests PASS + no console errors

---

## Step 3: Production Promote ✅

### Prerequisites (All Must Be True)
- ✅ Staging smoke tests: 7/7 PASS
- ✅ No critical errors in staging logs (last 24h)
- ✅ Backend health check: `/api/v1/health` returns 200 OK
- ✅ Python FastAPI health check: `/health` returns 200 OK
- ✅ UI loads without 404 errors (static files served)

### 3.1 Production Deployment

**Option A: Azure App Service (Blue-Green Deploy)**
```powershell
# Swap staging → production slot
az webapp deployment slot swap `
  --resource-group rg-skyworks-prod `
  --name app-skyworks-prod `
  --slot staging `
  --target-slot production

# Verify production URL
Start-Process "https://skyworks-prod.azurewebsites.net/app/Pages/ui/airspace-maps.html"
```

**Option B: Docker (with health check)**
```powershell
# Tag & push to registry
docker tag skyworks:v0.6.1 acrskyworks.azurecr.io/skyworks:v0.6.1
docker push acrskyworks.azurecr.io/skyworks:v0.6.1

# Deploy to production (Kubernetes/ACI)
kubectl set image deployment/skyworks-api skyworks=acrskyworks.azurecr.io/skyworks:v0.6.1

# Wait for rollout
kubectl rollout status deployment/skyworks-api
```

### 3.2 Post-Deploy Verification

```powershell
# Health checks
Invoke-RestMethod -Uri "https://skyworks-prod.azurewebsites.net/api/v1/health"
Invoke-RestMethod -Uri "https://skyworks-prod.azurewebsites.net:8001/health"

# UI accessibility
$response = Invoke-WebRequest -Uri "https://skyworks-prod.azurewebsites.net/app/Pages/ui/airspace-maps.html"
if ($response.StatusCode -eq 200) { Write-Host "✅ UI accessible" -ForegroundColor Green }

# Quick smoke test (1 test only)
# Import mission_facade.geojson → verify route appears
```

---

## Step 4: Optional Enhancements ✅

### 4.1 Enable Offline Tiles Cache

**Modify `airspace.js`**:
```javascript
// Add offline tile caching (IndexedDB)
const offlineCache = await caches.open('maplibre-tiles-v1');

map2D.on('load', async () => {
  const tileUrls = extractTileUrls(map2D);
  await offlineCache.addAll(tileUrls);
  console.log('Offline tiles cached:', tileUrls.length);
});
```

**Benefit**: UI works offline for previously viewed map areas

### 4.2 Add NOTAM Placeholder

**Modify `airspace.js`**:
```javascript
// NOTAM layer (placeholder for future NOTAMs feed)
function addNOTAMLayer() {
  map2D.addSource('notams', {
    type: 'geojson',
    data: {
      type: 'FeatureCollection',
      features: [] // Populate from NOTAM API
    }
  });

  map2D.addLayer({
    id: 'notams-layer',
    type: 'symbol',
    source: 'notams',
    layout: {
      'icon-image': 'warning-icon',
      'icon-size': 1.5
    }
  });
}
```

**Benefit**: Infrastructure ready for NOTAMs integration (Step 61+)

### 4.3 Enable Terrain for Cesium (Production Only)

**Add to Azure App Settings**:
```json
{
  "CESIUM_ION_TOKEN": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Update `airspace.js`**:
```javascript
// Read token from env (injected by backend)
Cesium.Ion.defaultAccessToken = window.CESIUM_ION_TOKEN || '';
```

**Backend injection** (in `Program.cs`):
```csharp
// Inject Cesium token into HTML
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/app/Pages/ui/airspace-maps.html"))
    {
        var html = await File.ReadAllTextAsync("WebPlatform/wwwroot/app/Pages/ui/airspace-maps.html");
        var token = Environment.GetEnvironmentVariable("CESIUM_ION_TOKEN") ?? "";
        html = html.Replace("</head>", $"<script>window.CESIUM_ION_TOKEN = '{token}';</script></head>");
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }
    else
    {
        await next();
    }
});
```

---

## Step 5: Monitoring & Rollback Plan ✅

### 5.1 Monitor Production (First 24h)

**Metrics to Watch**:
- Backend latency: `/api/v1/sora/calculate` response time < 2s
- Error rate: < 0.1% (4xx/5xx errors)
- Static file 404s: 0 (all UI assets served)
- Console errors: 0 critical JavaScript errors

**Logging**:
```powershell
# Azure App Service logs
az webapp log tail --resource-group rg-skyworks-prod --name app-skyworks-prod

# Application Insights (if configured)
az monitor app-insights query `
  --app skyworks-prod `
  --analytics-query "requests | where timestamp > ago(1h) | summarize count() by resultCode"
```

### 5.2 Rollback Procedure (if issues found)

**Option A: Revert Git Tag**
```powershell
# Find previous stable tag
git tag -l
# v0.6.0 (previous stable)

# Revert to v0.6.0
git checkout v0.6.0
git push origin main --force  # ⚠️ Use with caution
```

**Option B: Azure Slot Swap (Blue-Green)**
```powershell
# Swap back to previous production slot
az webapp deployment slot swap `
  --resource-group rg-skyworks-prod `
  --name app-skyworks-prod `
  --slot production `
  --target-slot staging
```

**Option C: Kubernetes Rollback**
```powershell
# Rollback to previous deployment
kubectl rollout undo deployment/skyworks-api

# Verify rollback
kubectl rollout status deployment/skyworks-api
```

---

## Timeline

| Step | Duration | Status |
|------|----------|--------|
| 1. Tag vX.Y.1 | 2 min | ⏳ |
| 2. Staging Deploy | 10 min | ⏳ |
| 3. Smoke Tests (7 steps) | 2 min | ⏳ |
| 4. Production Promote | 5 min | ⏳ |
| 5. Post-Deploy Verification | 3 min | ⏳ |
| **Total** | **22 min** | - |

---

## Success Criteria

- ✅ Tag v0.6.1 created & pushed to GitHub
- ✅ Staging environment running (no errors)
- ✅ Smoke tests: 7/7 PASS
- ✅ Production deployed (blue-green swap)
- ✅ Production health checks: all green
- ✅ No rollback required (first 24h monitoring)

---

## Team Notification

**Post to team channel** (Slack/Teams/GitHub):

```
🚀 **DEPLOYED: v0.6.1 - Mission Planner UI + Phase 6 Airspace Maps**

✅ **Staging**: Smoke tests 7/7 PASS
✅ **Production**: Deployed via blue-green swap
✅ **Tests**: 19/19 unit+golden, 18/18 e2e Playwright
✅ **Compliance**: EU-only terminology, ban-tokens clean

📦 **What's New**:
- 12-page Mission Planner UI (Annex B/AMC1 aligned)
- Phase 6 Airspace Maps (2D/3D toggle, routes, EU layers)
- Import/export GeoJSON/KML/CSV
- SORA compliance badges (iGRC/fGRC/iARC/rARC/SAIL)

🔗 **Try it**: https://skyworks-prod.azurewebsites.net/app/Pages/ui/airspace-maps.html

📊 **Monitoring**: Watch for errors (first 24h)
🔄 **Rollback**: Blue-green swap ready (if needed)

**Next**: Step 61 - Integration Phase (link airspace ↔ mission calculator)
```

---

## Next Steps (Step 61+)

1. **Integration Phase**: Link `airspace-maps.html` ↔ `mission.html`
   - Click "Analyze Mission" in airspace page → navigate to mission page with pre-filled geometry
   - Mission page → "View on Map" button → navigate to airspace page with route overlaid

2. **API Enhancement**: Extend `/api/v1/sora/calculate` to accept `missionGeometry`
   - Backend validates geometry (waypoints, geofence, CGA)
   - Returns enhanced response with geometry-based mitigations

3. **NOTAMs Feed**: Integrate real-time NOTAMs (Step 62)
   - Eurocontrol NOTAM API or equivalent
   - Display NOTAMs as icons on map with pop-up details

4. **Terrain Analysis**: Add terrain elevation checks (Step 63)
   - Validate altitude AGL vs AMSL
   - Warn if waypoints below terrain height
