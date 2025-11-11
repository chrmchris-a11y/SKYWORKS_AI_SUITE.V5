// NOTE: Google Maps paste UI is NOT implemented in mission.html yet.
// These tests are skipped intentionally and will be re-enabled in Phase X when the feature υλοποιηθεί.

import { test, expect } from '@playwright/test';

test.describe.skip('Google Maps Paste - Enhanced Parsing (UI not implemented)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/mission.html');
    await page.waitForLoadState('domcontentloaded');
    // Wait for wizard with increased timeout
    await page.waitForSelector('#missionWizard', { state: 'visible', timeout: 60000 });
    
    // Wait for templates to load
    await page.waitForFunction(() => {
      const select = document.getElementById('wizard-template') as HTMLSelectElement;
      return select && select.options.length > 1; // More than just "Select template..."
    }, { timeout: 10000 });
    
    // Navigate to step 2 - Use actual template code from MISSION_TEMPLATES
    await page.selectOption('#wizard-template', 'PhotovoltaicParkInspection');
    await page.click('button.wizard-next');
    await page.waitForSelector('.wizard-step[data-step="2"]', { state: 'visible' });
  });

  test('Simple lat/lon format: "52.5200, 13.4050"', async ({ page }) => {
    // Paste simple format
    await page.fill('#wizard-google-maps-input', '52.5200, 13.4050');
    await page.click('#wizard-parse-google-maps');
    
    // Wait for fields to populate
    await page.waitForTimeout(500);
    
    // Verify lat/lon populated
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    expect(parseFloat(lat)).toBeCloseTo(52.52, 2);
    expect(parseFloat(lon)).toBeCloseTo(13.405, 2);
  });

  test('Google Maps URL @pattern: https://www.google.com/maps/@52.5200,13.4050,15z', async ({ page }) => {
    // Paste URL with @ pattern
    await page.fill('#wizard-google-maps-input', 'https://www.google.com/maps/@52.5200,13.4050,15z');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForTimeout(500);
    
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    expect(parseFloat(lat)).toBeCloseTo(52.52, 2);
    expect(parseFloat(lon)).toBeCloseTo(13.405, 2);
  });

  test('Google Maps share link ?q= parameter: https://maps.app.goo.gl/?q=52.5200,13.4050', async ({ page }) => {
    // Paste share link with ?q=
    await page.fill('#wizard-google-maps-input', 'https://maps.app.goo.gl/?q=52.5200,13.4050');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForTimeout(500);
    
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    expect(parseFloat(lat)).toBeCloseTo(52.52, 2);
    expect(parseFloat(lon)).toBeCloseTo(13.405, 2);
  });

  test('Validation: Invalid latitude (>90)', async ({ page }) => {
    // Paste invalid lat
    await page.fill('#wizard-google-maps-input', '95.0, 13.4050');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForTimeout(500);
    
    // Fields should remain empty or show error
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    // Either empty (parsing failed) or unchanged
    expect(lat === '' || parseFloat(lat) !== 95.0).toBeTruthy();
  });

  test('Validation: Invalid longitude (<-180)', async ({ page }) => {
    // Paste invalid lon
    await page.fill('#wizard-google-maps-input', '52.5200, -185.0');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForTimeout(500);
    
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    // Either empty or invalid lon rejected
    expect(lon === '' || parseFloat(lon) !== -185.0).toBeTruthy();
  });

  test('Validation: Unrecognized format', async ({ page }) => {
    // Paste garbage text
    await page.fill('#wizard-google-maps-input', 'random text without coordinates');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForTimeout(500);
    
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    // Fields should remain empty (parsing failed)
    expect(lat === '' || lon === '').toBeTruthy();
  });
});
