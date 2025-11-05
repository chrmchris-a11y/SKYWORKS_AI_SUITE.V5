// Geometry utilities for basic geodesic and planar computations.
// SORA anchors: Steps 2–4 (operational volume, buffers, environment characterization)

export class GeometryUtils {
  // Haversine distance in meters (EPSG:4326 lat/lon degrees)
  static distanceMeters(a: [number, number], b: [number, number]): number {
    const R = 6371000 // Earth radius meters
    const toRad = (d: number) => (d * Math.PI) / 180
    const [lat1, lon1] = a
    const [lat2, lon2] = b
    const dLat = toRad(lat2 - lat1)
    const dLon = toRad(lon2 - lon1)
    const s1 = Math.sin(dLat / 2)
    const s2 = Math.sin(dLon / 2)
    const c = s1 * s1 + Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * s2 * s2
    const d = 2 * Math.atan2(Math.sqrt(c), Math.sqrt(1 - c))
    return R * d
  }

  // Planar polygon area (m^2) using Web Mercator approximate projection for small polygons.
  // For production, prefer robust libs (e.g., turf) and precise projections.
  static areaSqMetersWebMercator(coords: [number, number][]): number {
    if (coords.length < 3) return 0
    const projected = coords.map(([lat, lon]) => this.projectWebMercator(lat, lon))
    let sum = 0
    for (let i = 0, j = projected.length - 1; i < projected.length; j = i++) {
      const [xi, yi] = projected[i]
      const [xj, yj] = projected[j]
      sum += (xj + xi) * (yj - yi)
    }
    return Math.abs(sum / 2)
  }

  // EPSG:3857 (Web Mercator) projection for meters approximation
  static projectWebMercator(lat: number, lon: number): [number, number] {
    const originShift = 2 * Math.PI * 6378137 / 2.0
    const mx = (lon * originShift) / 180.0
    let my = Math.log(Math.tan(((90 + lat) * Math.PI) / 360.0)) / (Math.PI / 180.0)
    my = (my * originShift) / 180.0
    return [mx, my]
  }
}
