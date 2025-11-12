# Google Earth KML Samples

This directory contains sample KML files for testing the Google Earth import feature in SKYWORKS AI Suite.

## Files

### `route-sample.kml`
- **Type:** LineString (8 waypoints)
- **Purpose:** Example drone inspection route
- **Use Case:** Import as mission route for infrastructure survey
- **Coordinates:** Athens area (23.7275–23.7345°E, 37.9838–37.9852°N)

### `cga-sample.kml`
- **Type:** Polygon (5 vertices)
- **Purpose:** Example Controlled Ground Area (CGA)
- **Use Case:** Import as mission CGA boundary
- **Coordinates:** Athens area (rectangular polygon ~800m × 2km)

## How to Use

1. **Create Your Own KML in Google Earth:**
   - Open Google Earth Pro or Google Earth Web
   - Use the "Add Path" tool to draw your route
   - Or use "Add Polygon" tool to draw your CGA/geofence
   - Right-click → "Save Place As..." → Choose KML format
   - Save the file

2. **Import in SKYWORKS AI Suite:**
   - Navigate to **Airspace Maps** page (`http://localhost:5210/app/Pages/ui/airspace-maps.html`)
   - Find the **"🌍 Import from Google Earth (KML)"** section in the sidebar
   - Click "Choose File" and select your KML file
   - The route/polygon will be rendered on the map automatically

3. **Supported Geometry Types:**
   - ✅ **LineString**: Imported as mission route (blue polyline)
   - ✅ **Polygon**: Imported as CGA or geofence (yellow/red polygon)
   - ❌ **Point**: Not supported (use waypoint controls instead)
   - ❌ **MultiGeometry**: Partially supported (individual geometries extracted)

## Example Use Cases

**Power Line Inspection:**
```
Import route-sample.kml → 8 waypoints along power line corridor
```

**Agricultural Survey:**
```
Create polygon in Google Earth covering field → Import as CGA → Define mission area
```

**Emergency Response:**
```
Draw no-fly zone around incident → Import as geofence → Visualize restricted airspace
```

## Coordinate System

- **Format:** WGS84 (EPSG:4326)
- **Order:** Longitude, Latitude, Altitude (KML standard)
- **Conversion:** Automatically converted to GeoJSON [lon, lat] format

## Troubleshooting

**"Invalid XML/KML format" error:**
- Ensure file is valid XML (check with XML validator)
- Export directly from Google Earth (don't manually edit)

**"No valid LineString or Polygon features found" error:**
- Verify you used "Add Path" (for routes) or "Add Polygon" (for areas)
- Check that coordinates exist in the KML file (open in text editor)

**"Failed to parse KML file" error:**
- File may be corrupted during download/transfer
- Try re-exporting from Google Earth
- Ensure file extension is `.kml` (not `.kmz` compressed)

## Technical Details

**Parser Implementation:**
- Location: `WebPlatform/wwwroot/app/Pages/ui/assets/airspace.js`
- Function: `parseKmlToGeoJson(kmlText, filename)`
- Output: GeoJSON FeatureCollection compatible with MapLibre GL JS

**Supported KML Elements:**
- `<LineString><coordinates>` → GeoJSON LineString
- `<Polygon><outerBoundaryIs><coordinates>` → GeoJSON Polygon
- Altitude values ignored (2D coordinates only)

**Feature Properties:**
- `type`: "route" (LineString) or "cga" (Polygon)
- `name`: Derived from filename
- `source`: "google-earth-kml"
