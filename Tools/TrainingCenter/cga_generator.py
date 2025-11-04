#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
CGA Generator — produces simple GeoJSON and KML files for a Controlled Ground Area (CGA).

Inputs (one of):
- Explicit coordinates: --coords "lon,lat;lon,lat;lon,lat;..." (first/last will be closed automatically)
- Rect by center & size: --center "lon,lat" --width <meters> --height <meters>

Outputs to Docs/Compliance/CGA/<name>.geojson and <name>.kml

Note: No external deps, rough meter→degree conversion for small extents (~1e-5 deg per meter scaling
adjusted by latitude). Replace with accurate GIS workflow as needed.
"""
from __future__ import annotations
import argparse
import os
import math
from typing import List, Tuple

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT_DIR = os.path.join(ROOT, 'Docs', 'Compliance', 'CGA')


def parse_lonlat(s: str) -> Tuple[float, float]:
    lon_str, lat_str = s.split(',')
    return float(lon_str), float(lat_str)


def build_polygon_from_coords(coords_str: str) -> List[Tuple[float, float]]:
    pts: List[Tuple[float, float]] = []
    for part in coords_str.split(';'):
        part = part.strip()
        if not part:
            continue
        pts.append(parse_lonlat(part))
    if len(pts) < 3:
        raise ValueError('At least 3 points required for polygon')
    if pts[0] != pts[-1]:
        pts.append(pts[0])
    return pts


def build_rect(center_str: str, width_m: float, height_m: float) -> List[Tuple[float, float]]:
    lon_c, lat_c = parse_lonlat(center_str)
    lat_rad = math.radians(lat_c)
    # Approx: 1 deg lat ~ 111,320 m; 1 deg lon ~ 111,320 * cos(lat)
    dlat = (height_m / 2.0) / 111320.0
    dlon = (width_m / 2.0) / (111320.0 * max(0.2, math.cos(lat_rad)))
    pts = [
        (lon_c - dlon, lat_c - dlat),
        (lon_c + dlon, lat_c - dlat),
        (lon_c + dlon, lat_c + dlat),
        (lon_c - dlon, lat_c + dlat),
        (lon_c - dlon, lat_c - dlat),
    ]
    return pts


def to_geojson(name: str, pts: List[Tuple[float, float]]) -> str:
    coords = [[lon, lat] for (lon, lat) in pts]
    return (
        '{\n'
        f'  "type": "FeatureCollection",\n'
        f'  "name": "{name}",\n'
        '  "features": [\n'
        '    {\n'
        '      "type": "Feature",\n'
        '      "geometry": {\n'
        '        "type": "Polygon",\n'
        f'        "coordinates": [{coords}]\n'
        '      },\n'
        '      "properties": { "name": "CGA" }\n'
        '    }\n'
        '  ]\n'
        '}\n'
    )


def to_kml(name: str, pts: List[Tuple[float, float]]) -> str:
    coord_str = ' '.join([f"{lon},{lat},0" for (lon, lat) in pts])
    return f"""
<?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2">
  <Document>
    <name>{name}</name>
    <Placemark>
      <name>CGA</name>
      <Polygon>
        <outerBoundaryIs>
          <LinearRing>
            <coordinates>{coord_str}</coordinates>
          </LinearRing>
        </outerBoundaryIs>
      </Polygon>
    </Placemark>
  </Document>
</kml>
""".strip() + "\n"


def main():
    ap = argparse.ArgumentParser(description='CGA generator (GeoJSON/KML)')
    ap.add_argument('--name', required=True)
    ap.add_argument('--coords', default=None, help='"lon,lat;lon,lat;..." (closed automatically)')
    ap.add_argument('--center', default=None, help='"lon,lat" center of rectangle')
    ap.add_argument('--width', type=float, default=None, help='Width in meters (rect mode)')
    ap.add_argument('--height', type=float, default=None, help='Height in meters (rect mode)')
    args = ap.parse_args()

    if args.coords:
        pts = build_polygon_from_coords(args.coords)
    else:
        if not (args.center and args.width and args.height):
            raise SystemExit('Provide either --coords or all of --center, --width, --height')
        pts = build_rect(args.center, args.width, args.height)

    os.makedirs(OUT_DIR, exist_ok=True)
    base = args.name.replace(' ', '_')
    geojson_path = os.path.join(OUT_DIR, f"{base}.geojson")
    kml_path = os.path.join(OUT_DIR, f"{base}.kml")

    with open(geojson_path, 'w', encoding='utf-8') as f:
        f.write(to_geojson(args.name, pts))
    with open(kml_path, 'w', encoding='utf-8') as f:
        f.write(to_kml(args.name, pts))

    rel_gj = os.path.relpath(geojson_path, ROOT).replace('\\','/')
    rel_kml = os.path.relpath(kml_path, ROOT).replace('\\','/')
    print(f"✅ CGA files generated:\n- `{rel_gj}`\n- `{rel_kml}`\n")


if __name__ == '__main__':
    main()
