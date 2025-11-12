# 🌍 Google Earth KML Import - User Guide

**Version:** Phase 6 - Mission Planning & Airspace Maps  
**Last Updated:** 2025-11-10  
**Feature Location:** Airspace Maps page (`/app/Pages/ui/airspace-maps.html`)

---

## 📋 Table of Contents

1. [Purpose](#purpose)
2. [Prerequisites](#prerequisites)
3. [Step-by-Step User Guide](#step-by-step-user-guide)
4. [Supported Features](#supported-features)
5. [Limitations](#limitations)
6. [Sample Files](#sample-files)
7. [Errors & Troubleshooting](#errors--troubleshooting)
8. [EASA Compliance Note](#easa-compliance-note)

---

## 🎯 Purpose

The **Google Earth KML Import** feature allows operators to:

- Import pre-defined flight routes from Google Earth into SKYWORKS mission planning
- Import Controlled Ground Area (CGA) polygons for SORA risk assessment
- Visualize mission geometry on 2D/3D maps before EASA compliance checks
- Accelerate mission planning by reusing existing Google Earth placemarks

**Where to Find It:**
- Navigate to **Airspace Maps** page
- Look for the blue section: **"🌍 Import from Google Earth (KML)"**
- Located in the left sidebar under "Import Route"

---

## ✅ Prerequisites

Before using this feature:

1. **Google Earth Installed**
   - Google Earth Pro (desktop) or Google Earth Web
   - Version: Latest stable release recommended

2. **Supported File Format**
   - ✅ **KML** (Keyhole Markup Language) - `.kml` files
   - ❌ **KMZ** (compressed KML) - Not supported directly
   - 💡 **Workaround for KMZ:** Extract the `.kml` file from the `.kmz` archive using a ZIP tool

3. **Recommended Browsers**
   - Google Chrome (latest)
   - Microsoft Edge (latest)
   - Modern browsers with FileReader API support

4. **Projection System**
   - WGS84 (EPSG:4326) - Standard Google Earth coordinate system
   - Longitude/Latitude in decimal degrees

---

## 📝 Step-by-Step User Guide

### **Step 1: Create Geometry in Google Earth**

#### **For Flight Routes (LineString):**

1. Open Google Earth
2. Click **Add** → **Path** (or press `Ctrl+Shift+P`)
3. Enter a descriptive name (e.g., "Solar Farm Inspection Route")
4. Click on the map to add waypoints along your desired flight path
5. Recommended: 5-20 waypoints for smooth route visualization
6. Click **OK** to save

#### **For CGA Polygons:**

1. Open Google Earth
2. Click **Add** → **Polygon** (or press `Ctrl+Shift+G`)
3. Enter name (e.g., "CGA - Powerline Corridor")
4. Click on the map to define polygon vertices
5. Close the polygon by clicking near the first point
6. Adjust fill opacity/color if needed (visual only - not imported)
7. Click **OK** to save

### **Step 2: Export to KML**

1. In Google Earth sidebar, right-click on your Path/Polygon
2. Select **Save Place As...**
3. Choose file format: **KML** (not KMZ!)
4. Save to a memorable location (e.g., `Documents/SKYWORKS/Routes/`)
5. Note: File size should be <1MB for best performance

### **Step 3: Import into SKYWORKS**

1. Open SKYWORKS web interface
2. Navigate to **Airspace Maps** page
3. Locate the **"🌍 Import from Google Earth (KML)"** section (blue background)
4. Click **"Choose File"** or **"Browse"**
5. Select your exported `.kml` file
6. Click **Open**

### **Step 4: Verify Import**

**What You Should See:**

- **For Routes (LineString):**
  - Blue polyline connecting waypoints
  - Green **"S"** marker at start point
  - Red **"E"** marker at end point
  - Legend entry: "Route"

- **For CGA Polygons:**
  - Semi-transparent yellow fill (30% opacity)
  - Gold border outline
  - Legend entry: "CGA (Controlled Ground Area)"

- **Success Message:**
  - Green alert: "KML imported successfully"
  - Geometry statistics updated (waypoints count, path length, CGA area)

**If Nothing Appears:**
- Check browser console for errors (`F12` → Console tab)
- Verify KML file contains LineString or Polygon geometry
- Ensure coordinates are within valid ranges (lat: -90 to 90, lon: -180 to 180)

---

## 🛠️ Supported Features

### **Geometry Types**

| Type | Imported As | Map Visualization | Use Case |
|------|------------|------------------|----------|
| **LineString** | Mission Route | Blue polyline with S/E markers | Flight path, corridor inspection |
| **Polygon** | CGA or Geofence | Yellow fill + gold border | Controlled Ground Area, operational boundary |
| Point | ❌ Not supported | - | Use waypoint creator instead |
| MultiGeometry | ⚠️ Partial | First LineString/Polygon only | Use separate KML files |

### **Properties Recognized**

- **Coordinate System:** WGS84 (lon, lat, altitude)
- **Altitude Mode:** Clamped to ground (altitude values preserved but not used for 2D rendering)
- **Name/Description:** Imported for reference (shown in console logs)

### **Map Integration**

- **2D View:** MapLibre GL JS rendering
- **3D View:** CesiumJS rendering (future enhancement)
- **Legend:** Automatic entry added for imported layers
- **Export:** Imported geometry can be re-exported as GeoJSON/KML

---

## ⚠️ Limitations

### **File Format Restrictions**

1. **KMZ Not Supported:**
   - Compressed KML archives (`.kmz`) must be manually extracted
   - **Workaround:** Rename `.kmz` to `.zip`, extract `doc.kml`, rename to `.kml`

2. **Geometry Type Constraints:**
   - ❌ **Point** geometries ignored (use waypoint creator)
   - ❌ **MultiGeometry** only imports first valid LineString/Polygon
   - ⚠️ **Nested Placemarks:** Only top-level features processed

3. **File Size:**
   - Maximum recommended: **1 MB** (approx. 10,000 coordinates)
   - Larger files may cause browser performance issues

### **Coordinate Precision**

- **Decimal Places:** Limited to 6 decimal places (~0.1m precision)
- **Altitude:** Preserved in GeoJSON but not rendered in 2D view
- **Z-coordinates:** Ignored for CGA area calculations

### **Minimum Geometry Requirements**

- **LineString:** At least **2 points** (start + end)
- **Polygon:** At least **3 points** (triangle) + 1 closing point (total 4)
- Invalid geometries trigger error: "No valid LineString or Polygon found in KML"

### **Browser Compatibility**

- **FileReader API Required:** IE11 not supported
- **Large Files:** May timeout in Firefox <100MB RAM available
- **CORS Restrictions:** Files must be loaded from local disk (not remote URLs)

---

## 📦 Sample Files

SKYWORKS includes **2 sample KML files** for testing:

### **1. route-sample.kml**

**Location:** `Samples/kml/route-sample.kml`

**Description:**
- Demonstration flight route near Athens, Greece
- Use case: Powerline corridor inspection
- Geometry: 8-point LineString
- Approximate length: 2.5 km
- Coordinates: 37.9838°N, 23.7275°E (starting point)

**How to Use:**
1. Download from `Samples/kml/` folder
2. Import via Airspace Maps page
3. Expected result: Blue route with S/E markers
4. Verify: Route length ~2500m in geometry stats

### **2. cga-sample.kml**

**Location:** `Samples/kml/cga-sample.kml`

**Description:**
- Rectangular CGA polygon example
- Use case: SORA Controlled Ground Area definition
- Geometry: 5-point Polygon (rectangle)
- Approximate area: 0.5 km²
- Coordinates: 37.9700°N, 23.7200°E (center)

**How to Use:**
1. Download from `Samples/kml/` folder
2. Import via Airspace Maps page
3. Expected result: Yellow semi-transparent polygon
4. Verify: CGA area ~500,000 m² in geometry stats

**Testing Workflow:**
```
1. Import route-sample.kml → Verify blue route appears
2. Import cga-sample.kml → Verify yellow CGA polygon appears
3. Both geometries should coexist on map
4. Legend should show both "Route" and "CGA" entries
5. Export as GeoJSON → Verify both features in FeatureCollection
```

---

## 🚨 Errors & Troubleshooting

### **Common Error Messages**

#### **"Invalid XML/KML format"**

**Cause:** KML file is corrupted or not valid XML

**Solutions:**
1. Re-export from Google Earth (ensure "Save as KML" not "Save as KMZ")
2. Open KML in text editor → verify `<?xml version="1.0"?>` header
3. Validate XML syntax using online validator (e.g., xmlvalidation.com)
4. Check for special characters in name/description fields (escape `<`, `>`, `&`)

#### **"No valid LineString or Polygon found in KML"**

**Cause:** KML contains only Point geometries or no coordinates

**Solutions:**
1. Verify you created a **Path** (not Placemark) or **Polygon** in Google Earth
2. Open KML in text editor → search for `<LineString>` or `<Polygon>` tags
3. If only `<Point>` tags exist, recreate geometry as Path/Polygon
4. Ensure `<coordinates>` tag contains at least 2 (LineString) or 4 (Polygon) coordinate sets

#### **"Could not parse coordinates"**

**Cause:** Invalid coordinate format or out-of-range values

**Solutions:**
1. Verify coordinates are in format: `lon,lat,alt` (comma-separated, no spaces)
2. Check ranges: Latitude -90 to 90, Longitude -180 to 180
3. Example valid coordinate: `23.7275,37.9838,100`
4. Example invalid: `23.7275 37.9838` (space instead of comma)

#### **Map Shows Nothing After Import**

**Cause:** Geometry imported but outside current map view

**Solutions:**
1. Check browser console (`F12`) for errors
2. Verify success message appeared (green alert)
3. Check geometry statistics panel → if non-zero, import succeeded
4. **Fix:** Click "Fit to Mission Bounds" button (if available) or manually zoom out
5. Verify coordinates are in expected region (e.g., Europe, not Australia)

#### **Performance Issues / Browser Freeze**

**Cause:** KML file too large (>1MB) or too many coordinates

**Solutions:**
1. Simplify route in Google Earth (reduce waypoints from 1000 to <100)
2. Split large routes into multiple smaller KML files
3. Use desktop GIS tools (QGIS) to simplify geometry before export
4. Close other browser tabs to free memory

### **Browser-Specific Issues**

**Chrome/Edge:**
- If file dialog doesn't open → check popup blocker settings
- If import succeeds but no visualization → disable hardware acceleration

**Firefox:**
- Large files may timeout → reduce KML size or increase `dom.max_script_run_time`

**Safari:**
- FileReader API may reject files >10MB → use smaller files

---

## 📜 EASA Compliance Note

### **Purpose of KML Import**

The Google Earth KML import feature is a **mission planning tool** designed to:

- Accelerate route definition using familiar Google Earth interface
- Visualize mission geography before formal SORA assessment
- Import existing survey data from third-party sources

### **Compliance Responsibilities**

⚠️ **Important:** KML import does NOT guarantee EASA/JARUS compliance.

**Operator Responsibilities:**

1. **Verify Imported Coordinates:**
   - All coordinates must be validated against official aviation charts (ICAO, Eurocontrol)
   - Google Earth imagery may be outdated or inaccurate (±5-10m positional error)
   - Use certified GIS data for final submissions

2. **SORA Risk Assessment:**
   - Imported CGA polygons are **preliminary estimates**
   - Final GRC calculation requires ground population density data from official sources
   - Emergency landing sites must be physically verified (not based on satellite imagery alone)

3. **Airspace Compliance:**
   - Verify route does not intersect restricted/prohibited zones (CTR, P, R, D, TSA)
   - Check NOTAM for temporary restrictions
   - Obtain ATC clearances where required (Class A-D airspace)

4. **Documentation Requirements:**
   - Include KML source metadata in mission dossier (export date, author, data source)
   - Document any manual adjustments made after import
   - Maintain audit trail of geometry changes

### **Data Accuracy**

- **Google Earth Imagery:** Typically 1-3 years old, not real-time
- **Coordinate Precision:** ±1-10 meters horizontal accuracy (consumer GPS)
- **Altitude Data:** Google Earth elevation model has ±10-50m vertical error
- **Recommendation:** Use certified survey data for critical applications (transmission line proximity, urban BVLOS)

### **Final Approval**

All missions must undergo:
- Complete SORA 2.0/2.5 risk assessment (GRC, ARC, SAIL)
- OSO coverage verification (operational safety objectives)
- PDRA/STS conformance check (if applicable)
- National CAA approval (e.g., EASA, HCAA, DFS)

**The KML import tool is an aid, not a substitute for professional mission planning and regulatory compliance.**

---

## 📞 Support

**Documentation:**
- Mission Planner Guide: `Docs/MISSION_PLANNER.md`
- SORA Reference: `Docs/SORA_COMPLIANCE_REFERENCE.md`
- API Documentation: `Docs/API_REFERENCE.md`

**Sample Files:**
- `Samples/kml/route-sample.kml`
- `Samples/kml/cga-sample.kml`
- `Samples/kml/README.md`

**Contact:**
- Technical Support: support@skyworks.ai
- EASA Compliance Queries: compliance@skyworks.ai
- Training: training@skyworks.ai

---

**End of Document**  
*SKYWORKS AI Suite V5 - Phase 6 Mission Planning & Airspace Maps*
