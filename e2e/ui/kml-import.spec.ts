import { test, expect } from '@playwright/test';
import path from 'path';
import fs from 'fs';

test.describe('Google Earth KML Import', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    await page.waitForSelector('#map2D', { state: 'visible' });
  });

  test('Import route-sample.kml (LineString)', async ({ page }) => {
    // Set up file chooser listener
    const fileChooserPromise = page.waitForEvent('filechooser');
    
    // Click KML import button
    await page.locator('#importGoogleEarthKML').click();
    
    const fileChooser = await fileChooserPromise;
    const kmlPath = path.join(process.cwd(), 'Samples', 'kml', 'route-sample.kml');
    
    await fileChooser.setFiles(kmlPath);
    
    // Wait for success message in console
    await page.waitForTimeout(1000);
    
    // Verify route layer exists
    const routeVisible = await page.evaluate(() => {
      const map = (window as any).map2D;
      return map && map.getSource('mission-route') !== undefined;
    });
    
    expect(routeVisible).toBe(true);
    
    // Verify legend shows route
    const legendRoute = page.locator('.legend-label:has-text("Route")');
    await expect(legendRoute).toBeVisible();
  });

  test('Import cga-sample.kml (Polygon)', async ({ page }) => {
    const fileChooserPromise = page.waitForEvent('filechooser');
    
    await page.locator('#importGoogleEarthKML').click();
    
    const fileChooser = await fileChooserPromise;
    const kmlPath = path.join(process.cwd(), 'Samples', 'kml', 'cga-sample.kml');
    
    await fileChooser.setFiles(kmlPath);
    
    await page.waitForTimeout(1000);
    
    // Verify CGA layer exists
    const cgaVisible = await page.evaluate(() => {
      const map = (window as any).map2D;
      return map && map.getSource('mission-cga') !== undefined;
    });
    
    expect(cgaVisible).toBe(true);
    
    // Verify legend shows CGA
    const legendCGA = page.locator('.legend-label:has-text("CGA")');
    await expect(legendCGA).toBeVisible();
  });

  test('Error: Invalid KML (corrupt XML)', async ({ page }) => {
    // Create temporary invalid KML file
    const invalidKmlContent = '<?xml version="1.0"?><kml><INVALID></kml>';
    const tempFile = path.join(process.cwd(), 'temp-invalid.kml');
    
    fs.writeFileSync(tempFile, invalidKmlContent);
    
    const fileChooserPromise = page.waitForEvent('filechooser');
    await page.locator('#importGoogleEarthKML').click();
    
    const fileChooser = await fileChooserPromise;
    await fileChooser.setFiles(tempFile);
    
    // Wait for error in console
    await page.waitForTimeout(1000);
    
    // Cleanup
    fs.unlinkSync(tempFile);
  });

  test('Error: KML with no geometries', async ({ page }) => {
    // Create KML with no LineString/Polygon
    const emptyKmlContent = `<?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2">
  <Document>
    <name>Empty Document</name>
    <Placemark>
      <name>Point Only</name>
      <Point>
        <coordinates>13.4050,52.5200,0</coordinates>
      </Point>
    </Placemark>
  </Document>
</kml>`;
    
    const tempFile = path.join(process.cwd(), 'temp-empty.kml');
    fs.writeFileSync(tempFile, emptyKmlContent);
    
    const fileChooserPromise = page.waitForEvent('filechooser');
    await page.locator('#importGoogleEarthKML').click();
    
    const fileChooser = await fileChooserPromise;
    await fileChooser.setFiles(tempFile);
    
    // Wait for error in console
    await page.waitForTimeout(1000);
    
    // Cleanup
    fs.unlinkSync(tempFile);
  });

  test('Import both route + CGA simultaneously', async ({ page }) => {
    // Import route first
    let fileChooserPromise = page.waitForEvent('filechooser');
    await page.locator('#importGoogleEarthKML').click();
    
    let fileChooser = await fileChooserPromise;
    const routePath = path.join(process.cwd(), 'Samples', 'kml', 'route-sample.kml');
    await fileChooser.setFiles(routePath);
    
    await page.waitForTimeout(1000);
    
    // Import CGA second
    fileChooserPromise = page.waitForEvent('filechooser');
    await page.locator('#importGoogleEarthKML').click();
    
    fileChooser = await fileChooserPromise;
    const cgaPath = path.join(process.cwd(), 'Samples', 'kml', 'cga-sample.kml');
    await fileChooser.setFiles(cgaPath);
    
    await page.waitForTimeout(1000);
    
    // Verify both layers exist
    const bothVisible = await page.evaluate(() => {
      const map = (window as any).map2D;
      const routeExists = map && map.getSource('mission-route') !== undefined;
      const cgaExists = map && map.getSource('mission-cga') !== undefined;
      return { routeExists, cgaExists };
    });
    
    expect(bothVisible.routeExists).toBe(true);
    expect(bothVisible.cgaExists).toBe(true);
  });
});
