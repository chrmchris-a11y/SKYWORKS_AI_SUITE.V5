# Skyworks GIS Modules

This folder contains the GIS foundation per Step 51 — GIS Mapping System.

- EPSG:4326 and EPSG:3857 handling
- Layers: base maps, vector/raster (registry skeleton), tiled GeoJSON ready
- Drawing: to be added (polylines/polygons, snapping)
- Overlays: FG/OV/Buffer helpers (buffer helper included)
- Airspace overlays: CTR/TMZ/RMZ, Class A–G, UAS Geo Zones (data sources TBD)
- Caching/offline tiles: TODO
- Tests: GeometryUtils covered via Vitest

Compliance anchors:
- JARUS SORA 2.0 & 2.5 (Steps 2–4) — operational volume, buffers, environment characterization
- JARUS SORA Annex C — airspace class inputs (AEC → ARC mapping)
- EASA Easy Access Rules (EU 2019/947 & 2019/945), Art. 15 — UAS Geographical Zones (ingest/display)
