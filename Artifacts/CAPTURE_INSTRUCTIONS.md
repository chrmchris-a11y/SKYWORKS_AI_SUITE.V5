# Phase 6 Artifact Capture Instructions

## Prerequisites
1. Start Backend API: `cd Backend && dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210`
2. Open browser: `http://localhost:5210/app/Pages/ui/airspace-maps.html`

---

## Screenshot #1: Before (Map Center Only)
**File:** `auto_mission_before.png`

**Steps:**
1. Wait for Google Maps to load (Athens center: 37.9838, 23.7275)
2. Scroll down in left panel to "Create Complete 3D Mission" section
3. Verify all controls visible:
   - SORA version dropdown (default: JARUS SORA 2.5)
   - Mission template dropdown (default: VLOS Spot)
   - Numeric inputs (H_FG: 60m, Speed: 10 m/s, MTOM: 25 kg, Safe Radius: 500m)
   - 3D Mode switch (OFF)
   - "Create Complete 3D Mission" button (enabled)
4. **Capture:** Full browser window showing map (no SORA layers) + left panel with auto-mission controls
5. Save as: `Artifacts/auto_mission_before.png`

---

## Screenshot #2: After (FG/CV/GRB + Markers)
**File:** `auto_mission_after.png`

**Steps:**
1. Click **"Create Complete 3D Mission"** button
2. Wait ~1 second for rendering
3. Verify on map:
   - ✅ Green polygon (FG - Flight Geography)
   - ✅ Yellow polygon (CV - Contingency Volume)
   - ✅ Red polygon (GRB - Ground Risk Buffer)
   - ✅ Green marker "TOL" (Takeoff/Landing)
   - ✅ Purple markers "RP" (Remote Pilot) and "VO" (Visual Observer)
   - ✅ Orange markers "Observer 1", "Observer 2", "Observer 3"
   - ✅ Gray markers "E1", "E2", "E3" (Emergency Sites)
   - ✅ Distance labels (TOL→CV, Safe Radius, E1 distance)
4. **Capture:** Full map view showing all 3 layers + 9 markers + labels
5. Save as: `Artifacts/auto_mission_after.png`

---

## Screenshot #3: SORA Badge Panel
**File:** `sora_badge.png`

**Steps:**
1. After mission creation, scroll to bottom of left panel
2. Locate SORA version badge (should be green with text "JARUS SORA 2.5 Annex A")
3. **Capture:** Close-up of left panel showing:
   - Auto-mission input controls
   - SORA badge
   - Mission summary stats (if visible)
4. Save as: `Artifacts/sora_badge.png`

---

## KML Export
**File:** `auto_mission.kml`

**Steps:**
1. After mission creation, click **"Open in Google Earth"** button
2. Browser opens new tab with blob URL showing KML content
3. **Manual save:**
   - Press `Ctrl+S` (Save Page As)
   - Change filename to `auto_mission.kml`
   - Save to: `Artifacts/auto_mission.kml`
4. **Verify KML structure:**
   ```xml
   <?xml version="1.0" encoding="UTF-8"?>
   <kml xmlns="http://www.opengis.net/kml/2.2">
     <Document>
       <name>SORA Mission (JARUS SORA 2.5 Annex A)</name>
       <Placemark>
         <name>Flight Geography (FG)</name>
         <styleUrl>#fg-style</styleUrl>
         <Polygon>
           <extrude>1</extrude>
           <altitudeMode>relativeToGround</altitudeMode>
           ...
   ```

---

## JSON Dump
**File:** `auto_mission_sample.json`

**Steps:**
1. After mission creation, open browser console (F12)
2. Type: `copy(JSON.stringify(window.lastAutoMissionJson, null, 2))`
3. Press Enter
4. Open text editor, paste clipboard content
5. Save as: `Artifacts/auto_mission_sample.json`
6. **Verify JSON structure:**
   ```json
   {
     "flightGeography": { "type": "Feature", ... },
     "contingencyVolume": { "type": "Feature", ... },
     "groundRiskBuffer": { "type": "Feature", ... },
     "markers": {
       "tol": { "position": {...}, "label": "TOL", ... },
       "rp": { ... },
       "vo": { ... },
       "observers": [ ... ],
       "eSites": [ ... ]
     },
     "distances": [ ... ],
     "options": { ... }
   }
   ```

---

## Google Earth Preview (Optional)
**File:** `earth_preview.mp4` or `earth_preview.webm`

**Steps:**
1. Open downloaded `auto_mission.kml` in Google Earth Pro (desktop app)
2. Use screen recording tool (Windows Game Bar: Win+G)
3. Record 5-10 seconds showing:
   - KML loads with 3 extruded polygons (FG green, CV yellow, GRB red)
   - 9 marker placemarks
   - 3D extrusion at 60m height
4. Save as: `Artifacts/earth_preview.mp4`

---

## Automated Capture (PowerShell Script)

Run this from repository root:
```powershell
# Start backend
Start-Process pwsh -ArgumentList "-NoProfile -Command cd Backend ; dotnet run --project src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210"

# Wait for backend to start
Start-Sleep -Seconds 5

# Open browser (manual screenshot capture)
Start-Process msedge -ArgumentList "http://localhost:5210/app/Pages/ui/airspace-maps.html"

Write-Host "✅ Browser opened. Follow CAPTURE_INSTRUCTIONS.md to capture artifacts."
```

---

## Compliance Checklist

After capturing, verify:
- [ ] **Colors:** FG green (#16a34a), CV yellow (#f59e0b), GRB red (#ef4444)
- [ ] **Z-Index Order:** GRB (bottom), CV (middle), FG (top)
- [ ] **Landing Marker:** NOT present (dist(TOL, LND) = 0m < 50m)
- [ ] **RP/VO Placement:** RP at 0.8×safeRadius South, VO +30m East of RP
- [ ] **Observers:** 3 orange markers at 0°, 120°, 240°
- [ ] **E-sites:** 3 gray markers at 45°, 180°, 300°
- [ ] **Badge:** Green "JARUS SORA 2.5 Annex A" (matches dropdown)
- [ ] **Labels:** No overlaps (±12px repel applied)
- [ ] **Zero Console Errors:** F12 → Console tab should be clean

---

## Expected File List

After completion:
```
Artifacts/
├── auto_mission_before.png       # Map before creation
├── auto_mission_after.png        # Map with FG/CV/GRB + markers
├── sora_badge.png                # SORA badge panel
├── auto_mission.kml              # KML export with extrusions
├── auto_mission_sample.json      # window.lastAutoMissionJson dump
├── earth_preview.mp4             # (Optional) Google Earth video
└── CAPTURE_INSTRUCTIONS.md       # This file
```

---

**Note:** Screenshots must be captured manually as automated headless browsers cannot produce visual artifacts. Use Windows Snipping Tool (Win+Shift+S) or browser DevTools screenshot feature.
