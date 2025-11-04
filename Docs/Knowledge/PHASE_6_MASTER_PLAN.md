# Phase 6: Mission Planning & Maps – Master Plan

**Phase:** 6  
**Title:** Mission Planning & Maps (Βήματα 51–60)  
**Goal:** Ανάπτυξη συστήματος mission planning με GIS, route optimization, 3D mapping  
**SORA Compliance:** Dual support (EASA AMC 2.0 + JARUS SORA 2.5)  
**Start Date:** 2025-01-27  

---

## Overview

Phase 6 builds a comprehensive **Mission Planning & Mapping System** that integrates:
- GIS mapping with real-time airspace data
- Mission templates for common UAS operations
- Route optimization considering SORA constraints (ARC, SAIL, OSO)
- No-fly zones, airspace regulations, 3D terrain
- Full UI for mission planning and visualization

**Key Integration Points:**
- Phase 5 OSO/TMPR/Validation services (dual SORA)
- Existing airspace, weather, traffic providers
- Real-time telemetry streaming (Phase 4)
- PDRA/STS templates (Phase 3)

---

## Step-by-Step Plan

### **Step 51: GIS Mapping System**

**Objective:** Core GIS mapping infrastructure with dual SORA airspace layers

**Deliverables:**
1. **Models:**
   - `GISLayer` – airspace, no-fly zones, terrain, population
   - `MapFeature` – polygons, lines, points with SORA metadata
   - `Coordinate` – lat/lon/alt with validation
   - `BoundingBox` – geographic area selection

2. **Service: `GISMappingService`**
   - `GetAirspaceLayers(bbox, soraVersion)` → airspace features (CTR, restricted, etc.)
   - `GetNoFlyZones(bbox, soraVersion)` → no-fly polygons with SORA reasons
   - `GetPopulationDensity(bbox)` → density heatmap for GRC calculation
   - `GetTerrainElevation(coordinates)` → elevation profile for ARC height checks

3. **Controller: `GISController`**
   - `GET /api/gis/layers?bbox=...&soraVersion=2.5` → returns GeoJSON layers
   - `GET /api/gis/nofly?bbox=...` → no-fly zones
   - `GET /api/gis/elevation?points=...` → elevation data

4. **Documentation:**
   - `GIS_MAPPING_GUIDE.md` – integration with Leaflet/OpenLayers
   - Examples: rendering airspace with SORA 2.0 vs 2.5 color coding

5. **Tests:**
   - Bounding box validation
   - GeoJSON output format
   - Dual SORA airspace filtering

**Integration:**
- Use existing `AirspaceProvider` for CTR/FIR data
- Use `PopulationDensityProvider` for GRC heatmaps
- Extend with SORA version-aware filtering

**Acceptance:**
- [ ] GIS layers return valid GeoJSON
- [ ] Airspace features tagged with SORA version applicability
- [ ] No-fly zones include reason codes (military, airport, etc.)
- [ ] Build SUCCESS, tests PASSED

---

### **Step 52: Mission Templates**

**Objective:** Pre-configured mission templates for common operations (VLOS inspection, BVLOS survey, etc.)

**Deliverables:**
1. **Models:**
   - `MissionTemplate` – name, category, default params (SAIL, ARC, OSOs)
   - `TemplateParameter` – configurable fields (altitude, speed, area)
   - `TemplateConstraint` – SORA-specific limits (e.g., STS-01 max height)

2. **Service: `MissionTemplateService`**
   - `GetTemplates(category, soraVersion)` → list templates (PDRA-S01, STS-01, etc.)
   - `CreateMissionFromTemplate(templateId, params)` → instantiate mission
   - `ValidateTemplateCompliance(mission)` → check SORA constraints

3. **Controller: `MissionTemplateController`**
   - `GET /api/mission-templates?category=inspection&soraVersion=2.5`
   - `POST /api/mission-templates/{id}/instantiate`

4. **Documentation:**
   - `MISSION_TEMPLATES_GUIDE.md` – available templates, customization

5. **Templates to Create:**
   - VLOS Urban Inspection (SAIL II, ARC-b)
   - BVLOS Agricultural Survey (SAIL III, ARC-c, STS-02)
   - Emergency Response (SAIL IV, high priority)
   - Coastal Patrol (SAIL II, ARC-b, PDRA-S01)

**Integration:**
- Templates auto-fill OSO requirements via `OSOService`
- Pre-validate via `ValidationService`
- Link to PDRA/STS evaluators

**Acceptance:**
- [ ] 5+ templates available
- [ ] Templates enforce SORA constraints
- [ ] Instantiated missions valid for submission

---

### **Step 53: Route Optimization**

**Objective:** Optimize flight routes considering SORA risk, airspace, weather, and efficiency

**Deliverables:**
1. **Models:**
   - `Waypoint` – lat/lon/alt + timing + SORA risk score
   - `Route` – sequence of waypoints + total distance/time/risk
   - `RouteConstraint` – avoid zones, max altitude, min separation

2. **Service: `RouteOptimizationService`**
   - `OptimizeRoute(start, end, constraints, soraVersion)` → optimized waypoints
   - `CalculateRouteSAIL(route, soraVersion)` → composite SAIL (Phase 5 logic)
   - `ValidateRouteCompliance(route)` → check airspace/no-fly violations

3. **Algorithms:**
   - A* pathfinding with SORA risk heuristic
   - Avoid high-GRC areas (assemblies of people, high-density metro)
   - Minimize time in controlled airspace (reduce ATC coordination)
   - Weather-aware routing (avoid IMC, high winds)

4. **Controller: `RouteController`**
   - `POST /api/route/optimize` → { start, end, constraints, soraVersion }
   - `POST /api/route/validate` → check compliance

5. **Documentation:**
   - `ROUTE_OPTIMIZATION_GUIDE.md` – algorithms, examples

**Integration:**
- Use `AirspaceProvider` for airspace polygons
- Use `WeatherProvider` for VMC gates
- Use `PopulationDensityProvider` for GRC scoring
- Use `CompositeARCService` for segment-wise SAIL calculation

**Acceptance:**
- [ ] Routes avoid no-fly zones
- [ ] Routes minimize SORA risk (prefer lower GRC areas)
- [ ] Weather-aware routing functional
- [ ] Tests cover boundary cases (start/end in restricted zone)

---

### **Step 54: Mission Documentation**

**Objective:** Auto-generate mission documentation for regulatory submission (ConOps, Risk Assessment, Evidence)

**Deliverables:**
1. **Models:**
   - `MissionDocumentation` – ConOps, risk assessment, OSO evidence, TMPR docs
   - `DocumentSection` – title, content, SORA references

2. **Service: `MissionDocumentationService`**
   - `GenerateConOps(mission, soraVersion)` → Concept of Operations PDF/DOCX
   - `GenerateRiskAssessment(mission)` → GRC/ARC/SAIL summary + OSO compliance
   - `GenerateOSOEvidence(mission)` → per-OSO evidence checklist
   - `GenerateTMPRDocumentation(mission)` → TMPR systems, reliability calcs

3. **Templates:**
   - ConOps template (JARUS SORA format)
   - Risk Assessment template (Table 2, Table 5, M3 penalties)
   - OSO Evidence Matrix (Phase 5 OSODetailedRules)

4. **Controller: `MissionDocController`**
   - `POST /api/mission-doc/generate` → { missionId, docType, format }
   - `GET /api/mission-doc/{missionId}/conops.pdf`

5. **Documentation:**
   - `MISSION_DOCUMENTATION_GUIDE.md` – templates, customization

**Integration:**
- Pull mission data from database
- Use `ValidationService` for OSO compliance status
- Use `TMPRService` for TMPR requirements
- Use report generation from Phase 4 (QuestPDF)

**Acceptance:**
- [ ] ConOps PDF generated with mission details
- [ ] Risk Assessment includes dual SORA version note
- [ ] OSO evidence checklist auto-populated
- [ ] Documents ready for DCA Cyprus submission

---

### **Step 55: Real-time Maps**

**Objective:** Live map with real-time telemetry, traffic, weather overlays

**Deliverables:**
1. **Frontend:**
   - Leaflet/OpenLayers map component
   - Real-time drone position updates (WebSocket from Phase 4)
   - Traffic overlay (ADS-B targets from TrafficProvider)
   - Weather overlay (IMC/VMC zones, wind direction)

2. **Service: `RealTimeMapService`**
   - `GetLivePosition(droneId)` → current lat/lon/alt
   - `GetNearbyTraffic(position, radius)` → traffic targets
   - `GetWeatherOverlay(bbox)` → weather polygons

3. **WebSocket Events:**
   - `telemetry-update` → { droneId, lat, lon, alt, heading, speed }
   - `traffic-alert` → { targetId, distance, bearing }
   - `weather-change` → { zone, condition }

4. **Controller: `RealTimeMapController`**
   - `GET /api/realtime/position/{droneId}`
   - `GET /api/realtime/traffic?lat=...&lon=...&radius=5nm`

5. **Documentation:**
   - `REAL_TIME_MAPS_GUIDE.md` – WebSocket integration, map layers

**Integration:**
- Use existing `StreamingService` (Phase 4)
- Use `TrafficProvider` for ADS-B
- Use `WeatherProvider` for conditions

**Acceptance:**
- [ ] Map updates drone position every 1s
- [ ] Traffic targets displayed with distance/bearing
- [ ] Weather overlay shows IMC/VMC zones
- [ ] WebSocket connection stable

---

### **Step 56: No-Fly Zones Integration**

**Objective:** Comprehensive no-fly zone database with SORA-specific restrictions

**Deliverables:**
1. **Models:**
   - `NoFlyZone` – polygon, altitude range, reason, authority, SORA relevance
   - `RestrictionType` – military, airport, privacy, environmental, temporary

2. **Service: `NoFlyZoneService`**
   - `GetNoFlyZones(bbox, altitude, soraVersion)` → active restrictions
   - `CheckIntersection(route, soraVersion)` → violations
   - `GetAuthorityContact(zoneId)` → coordination info (for ATC)

3. **Data Sources:**
   - NOTAM feed (temporary restrictions)
   - Airport/heliport buffers (DCA Cyprus data)
   - Military zones (Cyprus SBA, restricted areas)
   - Environmental zones (Natura 2000, wildlife reserves)

4. **Controller: `NoFlyZoneController`**
   - `GET /api/nofly?bbox=...&alt=120`
   - `POST /api/nofly/check-route` → { route } → violations

5. **Documentation:**
   - `NO_FLY_ZONES_GUIDE.md` – data sources, update frequency

**Integration:**
- Use existing airspace data (Phase 3)
- Add NOTAM parsing
- Link to route validation (Step 53)

**Acceptance:**
- [ ] No-fly zones displayed on map
- [ ] Route validator detects violations
- [ ] SORA version filters applicable zones (e.g., 2.5 may allow certain operations)

---

### **Step 57: 3D Mapping**

**Objective:** 3D terrain and obstacle visualization for vertical risk assessment

**Deliverables:**
1. **Models:**
   - `TerrainMesh` – 3D elevation grid
   - `Obstacle` – buildings, towers, trees with height
   - `VerticalProfile` – route elevation cross-section

2. **Service: `3DMappingService`**
   - `GetTerrainMesh(bbox, resolution)` → 3D mesh data
   - `GetObstacles(bbox, minHeight)` → buildings/towers
   - `GenerateVerticalProfile(route)` → elevation chart

3. **Data Sources:**
   - SRTM/ASTER elevation data
   - OpenStreetMap buildings
   - FAA obstacle database (towers, wind turbines)

4. **Frontend:**
   - CesiumJS or Mapbox GL 3D rendering
   - Fly-through animation along route
   - Obstacle clearance visualization

5. **Controller: `3DMapController`**
   - `GET /api/3d/terrain?bbox=...&resolution=30m`
   - `GET /api/3d/obstacles?bbox=...`

6. **Documentation:**
   - `3D_MAPPING_GUIDE.md` – data formats, rendering tips

**Integration:**
- Use elevation data for ARC height checks (Phase 3)
- Validate route clearance above terrain + 500ft buffer

**Acceptance:**
- [ ] 3D terrain rendered on frontend
- [ ] Obstacles displayed with height labels
- [ ] Route validation includes vertical clearance checks

---

### **Step 58: Airspace Regulations**

**Objective:** Regulatory compliance engine for Cyprus/EASA airspace rules

**Deliverables:**
1. **Models:**
   - `AirspaceRegulation` – rule text, applicability, SORA version
   - `ComplianceCheck` – regulation ID, status, notes

2. **Service: `AirspaceRegulationService`**
   - `GetApplicableRegulations(mission, soraVersion)` → list of rules
   - `CheckCompliance(mission)` → pass/fail per regulation
   - `GetWaiverRequirements(violations)` → what to request from DCA

3. **Regulations Database:**
   - Cyprus DCA UAS regulations
   - EASA Easy Access Rules (Part-UAS)
   - SORA 2.0 AMC requirements
   - JARUS SORA 2.5 requirements
   - U-space regulations (when applicable)

4. **Controller: `RegulationController`**
   - `GET /api/regulations?mission={id}&soraVersion=2.5`
   - `POST /api/regulations/check-compliance`

5. **Documentation:**
   - `AIRSPACE_REGULATIONS_GUIDE.md` – Cyprus-specific rules

**Integration:**
- Link to `ValidationService` (Phase 5)
- Cross-reference OSO requirements
- Pre-flight checklist generation

**Acceptance:**
- [ ] Regulations database populated (Cyprus + EASA)
- [ ] Compliance checker identifies violations
- [ ] Waiver requirements auto-generated

---

### **Step 59: User Interface**

**Objective:** Unified mission planning UI with map, forms, validation, submission

**Deliverables:**
1. **Frontend Pages:**
   - `mission-planner.html` – main planning interface
   - `mission-library.html` – saved missions, templates
   - `mission-status.html` – live mission monitoring

2. **Components:**
   - Map canvas (Steps 51, 55, 57)
   - Mission form (template selection, params)
   - Route editor (drag waypoints, auto-optimize)
   - SORA version toggle (2.0 vs 2.5)
   - Validation panel (OSO checklist, violations)
   - Document generator (ConOps, Risk Assessment)

3. **Workflows:**
   - Create mission from template → customize → validate → submit
   - Import existing mission → edit route → re-validate
   - Live monitoring → replay telemetry → generate report

4. **Styling:**
   - Responsive design (desktop + tablet)
   - Dark/light mode
   - Greek + English i18n

5. **Documentation:**
   - `MISSION_PLANNER_UI_GUIDE.md` – user manual

**Integration:**
- All Phase 6 services (Steps 51–58)
- Phase 5 OSO/TMPR validation
- Phase 4 telemetry streaming

**Acceptance:**
- [ ] User can create mission in <5 min
- [ ] SORA version toggle changes validation rules
- [ ] Real-time validation feedback
- [ ] Documents downloadable as PDF

---

### **Step 60: Full Mission Suite**

**Objective:** End-to-end mission lifecycle – plan, validate, submit, execute, monitor, report

**Deliverables:**
1. **Mission Lifecycle Engine:**
   - Draft → Review → Submit → Approved → Active → Completed → Archived

2. **Service: `MissionLifecycleService`**
   - `CreateMission(template, params)` → draft mission
   - `SubmitForApproval(missionId)` → notify DCA Cyprus
   - `ApproveMission(missionId, authority)` → mark approved
   - `ActivateMission(missionId)` → enable live monitoring
   - `CompleteMission(missionId, telemetry)` → finalize, generate report

3. **Integration Points:**
   - Phase 3: PDRA/STS evaluation during submission
   - Phase 4: Telemetry streaming during execution
   - Phase 5: OSO/TMPR validation before submission
   - Phase 6: Mission docs attached to submission

4. **Submission API:**
   - `POST /api/mission/submit` → { missionId, documents, soraVersion }
   - Returns submission ID for tracking

5. **Monitoring Dashboard:**
   - Live mission status
   - Active missions map view
   - Alerts (airspace violations, weather changes)

6. **Post-Flight Report:**
   - Actual vs planned route comparison
   - SORA compliance verification (OSOs achieved)
   - Incidents/anomalies log
   - Evidence for next operation (build up operator track record)

7. **Documentation:**
   - `FULL_MISSION_SUITE_GUIDE.md` – end-to-end workflows
   - `PHASE_6_COMPLETION_REPORT.md` – summary of all 10 steps

**Integration:**
- All previous phases (1–5)
- All Phase 6 steps (51–59)

**Acceptance:**
- [ ] Mission lifecycle from draft to archived functional
- [ ] Submission to DCA Cyprus integrated (mock API)
- [ ] Live monitoring displays real-time telemetry
- [ ] Post-flight report auto-generated

---

## Phase 6 Acceptance Criteria

### Technical
- [ ] All 10 steps implemented (51–60)
- [ ] Build: SUCCESS (0 errors)
- [ ] Tests: >95% pass rate
- [ ] Dual SORA support in all mission services
- [ ] GIS/mapping functional with Leaflet/CesiumJS

### Compliance
- [ ] Cyprus DCA regulations integrated
- [ ] EASA Part-UAS compliance checks
- [ ] SORA 2.0 AMC and JARUS 2.5 validation
- [ ] Mission documentation ready for submission

### User Experience
- [ ] Mission creation <5 min
- [ ] Real-time map updates <1s latency
- [ ] Validation feedback immediate
- [ ] Greek + English UI

### Integration
- [ ] Phase 5 OSO/TMPR services used
- [ ] Phase 4 telemetry streaming active
- [ ] Phase 3 PDRA/STS templates linked
- [ ] All airspace/weather/traffic providers functional

---

## Timeline Estimate

| Step | Estimated Time | Dependencies |
|------|----------------|--------------|
| 51: GIS Mapping | 2–3 days | Existing AirspaceProvider |
| 52: Templates | 1–2 days | Phase 3 PDRA/STS |
| 53: Route Optimization | 3–4 days | Step 51, Phase 5 Validation |
| 54: Mission Docs | 2–3 days | Phase 5 OSO/TMPR, Phase 4 Reports |
| 55: Real-time Maps | 2–3 days | Step 51, Phase 4 Streaming |
| 56: No-Fly Zones | 2 days | Step 51 |
| 57: 3D Mapping | 3–4 days | Step 51, terrain data |
| 58: Regulations | 2 days | Cyprus DCA docs |
| 59: UI | 4–5 days | Steps 51–58 |
| 60: Full Suite | 3–4 days | All previous steps |

**Total Estimated:** ~25–35 days (can be parallelized)

---

## Next Steps

1. **Immediate:** Start Step 51 (GIS Mapping System)
2. **Week 1:** Complete Steps 51–53 (GIS, Templates, Route Opt)
3. **Week 2:** Complete Steps 54–56 (Docs, Real-time, No-Fly)
4. **Week 3:** Complete Steps 57–58 (3D, Regulations)
5. **Week 4:** Complete Steps 59–60 (UI, Full Suite)

**Success Metric:** By end of Phase 6, a pilot can:
- Select a mission template
- Customize route on a map
- Auto-validate SORA compliance
- Generate submission docs
- Submit to DCA Cyprus
- Monitor live execution
- Receive post-flight report

---

**Phase 6 Start:** 2025-01-27  
**Target Completion:** 2025-02-28  
**Owner:** Skyworks Development Team
