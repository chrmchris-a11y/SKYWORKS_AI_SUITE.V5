/**
 * Global type declarations for E2E tests
 * Extends Window interface with airspace.js globals
 */

declare global {
  interface Window {
    // Map instances
    map2D: any;
    viewer3D: any;
    
    // Test data
    testMissionData: any;
    testErpData: any;
    
    // Functions from airspace.js
    renderMission: (data: any) => void;
    updateErpPanel: (erp: any) => void;
    updateSoraBadges: (sora: any) => void;
    updateOsoPanel: (oso: any) => void;
    parseGoogleMapsInput: (input: string) => { lat: number; lon: number } | null;
    importGeoJSON: (geojson: any) => void;
    handleGoogleEarthKMLImport: (event: Event) => void;
  }
}

export {};
