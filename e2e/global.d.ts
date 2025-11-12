/**
 * Global Type Declarations for E2E Tests
 * Declares Google Maps API types and custom window properties
 */

// Google Maps API global object (loaded via script tag in browser)
declare const google: typeof import('@types/google.maps').google;

// MapLibre GL (checking absence in tests)
declare const maplibregl: any;

// Leaflet (checking absence in tests)
declare const L: any;

// Cesium (checking absence in tests)
declare const Cesium: any;

// Global map2D variable (created in airspace.js)
declare const map2D: google.maps.Map | undefined;

// Custom window properties added by airspace.js
interface Window {
  checkGoogleMapsHealth(): boolean;
  parseGoogleMapsUrl(url: string): { lat: number; lng: number; zoom?: number } | null;
  geocodeLatLng(lat: number, lng: number): Promise<string>;
  geocodeAddress(address: string): Promise<{ lat: number; lng: number } | null>;
  toggle2DOblique(): void;
  initGoogleMaps(): void;
  
  // Phase 6: Auto-Mission properties
  fgPolygon: google.maps.Polygon | null;
  cvPolygon: google.maps.Polygon | null;
  grbPolygon: google.maps.Polygon | null;
  autoMissionMarkers: google.maps.Marker[] | null;
  lastAutoMissionJson: {
    flightGeography: any;
    contingencyVolume: any;
    groundRiskBuffer: any;
    markers: any;
    distances: any;
    options: any;
  } | null;
  autoMissionState: {
    isReady: boolean;
    USE_CESIUM_3D: boolean;
    currentKmlBlobUrl: string | null;
  } | null;
}
