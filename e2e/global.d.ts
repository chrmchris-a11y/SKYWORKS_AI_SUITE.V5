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
}
