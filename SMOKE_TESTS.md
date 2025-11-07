# 🧪 Manual Smoke Tests - Phase 6 Airspace Maps
## Estimated Time: 2 minutes total

**Prerequisites**:
- Backend API running on http://localhost:5210
- Python FastAPI running on http://localhost:8001 (for SORA calculations)
- Browser: Edge/Chrome with dev tools open (F12 → Console tab)

---

## Test 1: Import GeoJSON ✅
**Duration**: 15 seconds

1. Navigate to http://localhost:5210/app/Pages/ui/airspace-maps.html
2. Click **"Import File"** button in sidebar
3. Select `WebPlatform/wwwroot/app/Pages/ui/assets/samples/mission_facade.geojson`
4. **Expected**:
   - ✅ Route appears on map (6 blue waypoint markers)
   - ✅ CGA polygon appears (green outline around building)
   - ✅ Geometry stats show "Waypoints: 6"
   - ✅ Console logs "Imported 6 waypoints + CGA polygon"

**Pass/Fail**: ________

---

## Test 2: Toggle 2D/3D ✅
**Duration**: 10 seconds

1. Click **"Switch to 3D"** button in header
2. Wait for Cesium viewer to load (2-3 seconds)
3. Verify camera fly-to mission center (Berlin area)
4. Click **"Switch to 2D"** to toggle back
5. **Expected**:
   - ✅ Map transitions smoothly (no errors in console)
   - ✅ 3D viewer shows terrain + waypoints as point entities
   - ✅ 2D map shows same waypoints as markers
   - ✅ No JavaScript errors in console

**Pass/Fail**: ________

---

## Test 3: Layers (EU Airspace) ✅
**Duration**: 20 seconds

1. In sidebar, expand **"Layers"** section
2. Enable **RMZ** checkbox → verify blue polygon appears on map
3. Enable **CTR** checkbox → verify orange polygon appears on map
4. Enable **UAS Geo Zone** checkbox → verify purple polygon appears on map
5. Disable all checkboxes → verify polygons disappear
6. **Expected**:
   - ✅ Each layer renders with correct color
   - ✅ Layers toggle ON/OFF independently
   - ✅ No Z-fighting or rendering glitches
   - ✅ Console logs layer visibility changes

**Pass/Fail**: ________

---

## Test 4: Draw (Waypoint, Geofence, CGA) ✅
**Duration**: 30 seconds

1. Click **"Add Waypoint"** button
2. Enter coordinates: Lat `52.52`, Lon `13.405`, Alt `50` → Click Add
3. Verify waypoint marker appears on map
4. Click **"Draw Geofence"** button
5. Verify geofence circle appears (500m radius, 150m height)
6. Click **"Draw CGA"** button
7. Verify CGA polygon appears (bounding box around waypoints)
8. **Expected**:
   - ✅ Waypoint marker renders at correct location
   - ✅ Geometry stats update: "Waypoints: 1"
   - ✅ Geofence stats: "500m radius, 150m height"
   - ✅ CGA stats: "Area: XXX m²"
   - ✅ All geometry visible on map

**Pass/Fail**: ________

---

## Test 5: SORA POST (Analyze Mission) ✅
**Duration**: 20 seconds

1. With route/geofence/CGA drawn, click **"Analyze Mission"** button
2. Wait for API response (1-2 seconds)
3. Check right panel → SORA Compliance badges
4. **Expected**:
   - ✅ Initial GRC badge updates (green = I-II, yellow = III-IV, red = V-VI)
   - ✅ Final GRC badge updates
   - ✅ Initial ARC badge updates
   - ✅ Residual ARC badge updates
   - ✅ SAIL badge updates (I-VI color-coded)
   - ✅ Console logs request/response JSON

**Response Example**:
```json
{
  "initialGrc": 3,
  "finalGrc": 2,
  "arc": { "initial": 4, "residual": 2 },
  "sail": 2
}
```

**Pass/Fail**: ________

---

## Test 6: Export (GeoJSON/KML/CSV) ✅
**Duration**: 15 seconds

1. In sidebar, expand **"Export"** section
2. Click **"Export GeoJSON"** → verify `mission.geojson` downloads
3. Click **"Export KML"** → verify `mission.kml` downloads
4. Click **"Export CSV"** → verify `mission.csv` downloads
5. Open downloaded files → verify content is valid
6. **Expected**:
   - ✅ GeoJSON contains FeatureCollection with waypoints + CGA
   - ✅ KML contains Placemarks for waypoints
   - ✅ CSV contains "lat,lon,alt_m" rows
   - ✅ All files non-empty and valid format

**GeoJSON Structure**:
```json
{
  "type": "FeatureCollection",
  "features": [
    { "type": "Feature", "geometry": { "type": "Point", "coordinates": [13.405, 52.52, 50] } },
    { "type": "Feature", "geometry": { "type": "Polygon", "coordinates": [[...]] } }
  ]
}
```

**Pass/Fail**: ________

---

## Test 7: Console (Validation Output) ✅
**Duration**: 10 seconds

1. Scroll to bottom of right panel → Validation Console
2. Verify console shows:
   - "Airspace Maps initialized (2D mode)"
   - "Imported X waypoints + CGA polygon"
   - "POST /api/v1/sora/calculate → 200 OK"
   - Request/response JSON formatted
3. **Expected**:
   - ✅ All operations logged with timestamps
   - ✅ Success messages in green
   - ✅ Warnings in yellow
   - ✅ Errors in red (if any)
   - ✅ JSON formatted with syntax highlighting

**Pass/Fail**: ________

---

## Summary

| Test | Status | Notes |
|------|--------|-------|
| 1. Import GeoJSON | ☐ | Route + CGA visible |
| 2. Toggle 2D/3D | ☐ | No errors, smooth transition |
| 3. Layers | ☐ | RMZ/CTR/UAS Geo Zones render |
| 4. Draw | ☐ | Waypoint/Geofence/CGA stats update |
| 5. SORA POST | ☐ | Badges update from API response |
| 6. Export | ☐ | GeoJSON/KML/CSV download valid |
| 7. Console | ☐ | All logs visible, JSON formatted |

**Overall Result**: ☐ PASS  ☐ FAIL

---

## Notes

- If Cesium 3D terrain doesn't load → check `CESIUM_ION_TOKEN` env var
- If SORA POST fails → verify Python FastAPI running on port 8001
- If layers don't render → check browser console for CORS errors
- If exports are empty → verify route/geofence/CGA drawn before export

---

## Ban Tokens Verification (Critical)

**Quick Scan** (press Ctrl+F in browser):
- Search "ACE" → ❌ MUST NOT EXIST
- Search "Mode-S veil" → ❌ MUST NOT EXIST
- Search "CTR" → ✅ MUST EXIST (EU airspace term)
- Search "RMZ" → ✅ MUST EXIST (EU airspace term)

**Expected**: No banned tokens, EU-only terminology enforced.

---

## Post-Smoke Actions

If all 7 tests PASS:
1. Update MERGE_CHECKLIST.md → mark smoke tests ✅ VERIFIED
2. Add smoke test results to PR comment
3. Proceed to merge (squash & merge strategy)
4. Tag vX.Y.1
5. Deploy to staging
6. Promote to production
