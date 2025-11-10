import { test, expect } from '@playwright/test';

test.describe('Google Maps Paste - Enhanced Parsing', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/mission.html');
    await page.waitForSelector('#missionWizard', { state: 'visible' });
    
    // Navigate to step 2
    await page.selectOption('#wizard-template', 'InfrastructureInspection');
    await page.click('button.wizard-next');
  });

  test('Simple lat/lon format: "52.5200, 13.4050"', async ({ page }) => {
    // Paste simple format
    await page.fill('#wizard-google-maps-input', '52.5200, 13.4050');
    await page.click('#wizard-parse-google-maps');
    
    // Wait for success message
    await page.waitForSelector('.alert-success', { state: 'visible', timeout: 3000 });
    
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
    
    await page.waitForSelector('.alert-success', { state: 'visible', timeout: 3000 });
    
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    expect(parseFloat(lat)).toBeCloseTo(52.52, 2);
    expect(parseFloat(lon)).toBeCloseTo(13.405, 2);
  });

  test('Google Maps share link ?q= parameter: https://maps.app.goo.gl/?q=52.5200,13.4050', async ({ page }) => {
    // Paste share link with ?q=
    await page.fill('#wizard-google-maps-input', 'https://maps.app.goo.gl/?q=52.5200,13.4050');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForSelector('.alert-success', { state: 'visible', timeout: 3000 });
    
    const lat = await page.inputValue('#wizard-lat');
    const lon = await page.inputValue('#wizard-lon');
    
    expect(parseFloat(lat)).toBeCloseTo(52.52, 2);
    expect(parseFloat(lon)).toBeCloseTo(13.405, 2);
  });

  test('Validation: Invalid latitude (>90)', async ({ page }) => {
    // Paste invalid lat
    await page.fill('#wizard-google-maps-input', '95.0, 13.4050');
    await page.click('#wizard-parse-google-maps');
    
    // Wait for error message
    await page.waitForSelector('.alert-error', { state: 'visible', timeout: 3000 });
    
    const errorText = await page.locator('.alert-error').textContent();
    expect(errorText).toContain('Latitude must be between -90 and 90');
  });

  test('Validation: Invalid longitude (<-180)', async ({ page }) => {
    // Paste invalid lon
    await page.fill('#wizard-google-maps-input', '52.5200, -185.0');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForSelector('.alert-error', { state: 'visible', timeout: 3000 });
    
    const errorText = await page.locator('.alert-error').textContent();
    expect(errorText).toContain('Longitude must be between -180 and 180');
  });

  test('Validation: Unrecognized format', async ({ page }) => {
    // Paste garbage text
    await page.fill('#wizard-google-maps-input', 'random text without coordinates');
    await page.click('#wizard-parse-google-maps');
    
    await page.waitForSelector('.alert-error', { state: 'visible', timeout: 3000 });
    
    const errorText = await page.locator('.alert-error').textContent();
    expect(errorText).toContain('Could not parse coordinates');
  });
});
