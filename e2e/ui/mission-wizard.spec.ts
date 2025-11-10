import { test, expect } from '@playwright/test';

test.describe('Mission Wizard E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/mission.html');
    await page.waitForSelector('#missionWizard', { state: 'visible' });
  });

  test('Full flow: PhotovoltaicParkInspection creation', async ({ page }) => {
    // Step 1: Select template
    await page.selectOption('#wizard-template', 'PhotovoltaicParkInspection');
    await expect(page.locator('#template-preview')).toBeVisible();
    await expect(page.locator('#preview-category')).toContainText('EnergyAndUtilities');
    await page.click('button.wizard-next');

    // Step 2: Location & Drone
    await page.fill('#wizard-lat', '52.5200');
    await page.fill('#wizard-lon', '13.4050');
    await page.fill('#wizard-height', '100');
    await page.fill('#wizard-drone-model', 'DJI Mavic 3 Enterprise');
    await page.fill('#wizard-drone-mtom', '0.9');
    await page.selectOption('#wizard-drone-class', 'C2');
    await page.click('button.wizard-next');

    // Step 3: Confirmation
    await expect(page.locator('#wizard-summary')).toContainText('PhotovoltaicParkInspection');
    await expect(page.locator('#wizard-summary')).toContainText('52.5200, 13.4050');
    
    // Create mission
    await page.click('#wizard-create');
    await page.waitForSelector('#wizard-result', { state: 'visible', timeout: 15000 });
    
    const missionIdText = await page.locator('#result-mission-id').textContent();
    expect(missionIdText).toMatch(/[a-f0-9-]{36}/); // GUID format
    
    const mapsLink = page.locator('#link-maps');
    await expect(mapsLink).toHaveAttribute('href', /airspace-maps\.html\?missionId=/);
  });

  test('Validation: Missing required fields', async ({ page }) => {
    // Step 1: No template selected
    await page.click('button.wizard-next');
    page.on('dialog', async dialog => {
      expect(dialog.message()).toContain('select a template');
      await dialog.accept();
    });

    await page.selectOption('#wizard-template', 'TrainingFlightVLOS');
    await page.click('button.wizard-next');

    // Step 2: Missing drone data
    await page.fill('#wizard-lat', '48.8566');
    await page.fill('#wizard-lon', '2.3522');
    await page.click('button.wizard-next');
    
    page.on('dialog', async dialog => {
      expect(dialog.message()).toContain('fill all required fields');
      await dialog.accept();
    });
  });

  test('Google Maps paste parsing', async ({ page }) => {
    await page.selectOption('#wizard-template', 'FacadeInspection');
    await page.click('button.wizard-next');

    // Paste coordinates in "lat, lon" format
    await page.fill('#gmaps-paste-input', '40.7128, -74.0060');
    await page.click('#btn-parse-gmaps');

    await expect(page.locator('#wizard-lat')).toHaveValue('40.7128');
    await expect(page.locator('#wizard-lon')).toHaveValue('-74.0060');
  });

  test('Maps load by missionId parameter', async ({ page }) => {
    // Create a mission first
    await page.selectOption('#wizard-template', 'BridgeStructuralInspection');
    await page.click('button.wizard-next');
    await page.fill('#wizard-lat', '51.5074');
    await page.fill('#wizard-lon', '-0.1278');
    await page.fill('#wizard-height', '120');
    await page.fill('#wizard-drone-model', 'DJI M300 RTK');
    await page.fill('#wizard-drone-mtom', '6.3');
    await page.selectOption('#wizard-drone-class', 'C3');
    await page.click('button.wizard-next');
    await page.click('#wizard-create');
    
    await page.waitForSelector('#wizard-result', { state: 'visible', timeout: 15000 });
    const missionId = await page.locator('#result-mission-id').textContent();

    // Navigate to airspace maps
    await page.goto(`http://localhost:5210/app/Pages/airspace-maps.html?missionId=${missionId}`);
    await page.waitForSelector('#map2D', { state: 'visible' });
    
    // Check SORA badges updated
    const igrcBadge = page.locator('#badge-igrc');
    await expect(igrcBadge).not.toBeEmpty();
    
    // Check ERP panel
    const erpPanel = page.locator('#erp-panel');
    await expect(erpPanel).toBeVisible();
  });

  test('Navigation: Back buttons work', async ({ page }) => {
    await page.selectOption('#wizard-template', 'SolarPanelCleaning');
    await page.click('button.wizard-next');
    
    await expect(page.locator('[data-step="2"]')).toBeVisible();
    
    await page.click('button.wizard-prev');
    await expect(page.locator('[data-step="1"]')).toBeVisible();
  });
});
