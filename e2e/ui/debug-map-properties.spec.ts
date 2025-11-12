import { test, expect } from '@playwright/test';

test.describe('Debug Map Properties', () => {
  test.beforeEach(async ({ page }) => {
    page.on('console', msg => console.log('[BROWSER]', msg.text()));
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForSelector('#map2D', { state: 'visible', timeout: 10000 });
  });

  test('Map initializes and sources are created', async ({ page }) => {
    // Wait for map to fully load
    await page.waitForTimeout(2000);
    
    const mapStatus = await page.evaluate(() => {
      const map = (window as any).map2D;
      
      if (!map) {
        return {
          mapExists: false,
          error: 'map2D not found on window'
        };
      }
      
      // Check if map is loaded
      const isLoaded = map.loaded();
      
      // Get all sources
      const style = map.getStyle();
      const sourceIds = style?.sources ? Object.keys(style.sources) : [];
      
      // Get all layers
      const layerIds = style?.layers ? style.layers.map((l: any) => l.id) : [];
      
      return {
        mapExists: true,
        isLoaded,
        sourceIds,
        layerIds,
        hasRMZLayer: layerIds.includes('rmz-fill'),
        hasTMZLayer: layerIds.includes('tmz-fill'),
        hasCTRLayer: layerIds.includes('ctr-fill')
      };
    });
    
    console.log('Map Status:', JSON.stringify(mapStatus, null, 2));
    
    // Assertions
    expect(mapStatus.mapExists).toBe(true);
    
    // Type guard: only check these if map exists
    if ('isLoaded' in mapStatus && mapStatus.sourceIds && mapStatus.layerIds) {
      expect(mapStatus.isLoaded).toBe(true);
      expect(mapStatus.sourceIds.length).toBeGreaterThan(0);
      expect(mapStatus.layerIds.length).toBeGreaterThan(0);
    }
  });

  test('RenderMission creates mission layers', async ({ page }) => {
    // Create test mission data with 3 waypoints (minimum for route)
    await page.evaluate(() => {
      window.testMissionData = {
        waypoints: [
          { lat: 52.5200, lon: 13.4050, alt: 100 },
          { lat: 52.5250, lon: 13.4100, alt: 120 },
          { lat: 52.5300, lon: 13.4150, alt: 110 }
        ]
      };
    });
    
    // Call renderMission if it exists
    const renderResult = await page.evaluate(() => {
      const map = (window as any).map2D;
      const renderMission = (window as any).renderMission;
      
      if (!renderMission) {
        return { error: 'renderMission not found' };
      }
      
      try {
        renderMission(window.testMissionData);
        
        // Check what sources were created
        const style = map.getStyle();
        const sourceIds = Object.keys(style.sources);
        const layerIds = style.layers.map((l: any) => l.id);
        
        return {
          success: true,
          hasMissionStart: sourceIds.includes('mission-start'),
          hasMissionEnd: sourceIds.includes('mission-end'),
          sourceCount: sourceIds.length,
          layerCount: layerIds.length,
          allSources: sourceIds,
          allLayers: layerIds
        };
      } catch (error: any) {
        return {
          error: error.message
        };
      }
    });
    
    console.log('Render Result:', JSON.stringify(renderResult, null, 2));
    
    // Assertions - Check that renderMission executed successfully
    expect(renderResult.success).toBe(true);
    // Check that at least start and end sources were created
    expect(renderResult.hasMissionStart).toBe(true);
    expect(renderResult.hasMissionEnd).toBe(true);
    // Verify sources and layers were added
    expect(renderResult.sourceCount).toBeGreaterThan(2); // At least osm + mission-start + mission-end
  });
});
