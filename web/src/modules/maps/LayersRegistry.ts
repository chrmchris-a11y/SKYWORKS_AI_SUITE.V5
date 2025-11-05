// LayersRegistry: central layer registration and toggling
// Supports: base maps, vector/raster layers, tiled GeoJSON, clustering, heatmaps (skeleton)
import L, { Layer } from 'leaflet'

export type LayerKey = string

export class LayersRegistry {
  private layers = new Map<LayerKey, Layer>()

  register(key: LayerKey, layer: Layer) {
    this.layers.set(key, layer)
  }

  addToMap(key: LayerKey, map: L.Map) {
    const layer = this.layers.get(key)
    if (layer) layer.addTo(map)
  }

  removeFromMap(key: LayerKey, map: L.Map) {
    const layer = this.layers.get(key)
    if (layer) map.removeLayer(layer)
  }

  toggle(key: LayerKey, map: L.Map, on?: boolean) {
    const layer = this.layers.get(key)
    if (!layer) return
    const has = map.hasLayer(layer)
    const shouldAdd = on ?? !has
    if (shouldAdd && !has) layer.addTo(map)
    if (!shouldAdd && has) map.removeLayer(layer)
  }
}
