// MapService: wraps map init and provides helpers for overlays (FG/OV/Buffer)
import L from 'leaflet'

export class MapService {
  private map?: L.Map
  init(el: HTMLElement, center: [number, number] = [37.9838, 23.7275], zoom = 12) {
    this.map = L.map(el).setView(center, zoom)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map)
    return this.map
  }

  get(): L.Map {
    if (!this.map) throw new Error('Map not initialized')
    return this.map
  }

  addBuffer(lat: number, lon: number, radiusMeters: number, opts?: L.CircleMarkerOptions) {
    const m = this.get()
    return L.circle([lat, lon], { radius: radiusMeters, ...opts }).addTo(m)
  }
}
