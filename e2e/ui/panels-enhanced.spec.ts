import { test, expect } from '@playwright/test';

test.describe('Enhanced Panels (ERP, SORA, OSO)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    await page.waitForSelector('#map2D', { state: 'visible' });
  });

  test('ERP Panel: 5 sections rendered', async ({ page }) => {
    // Load mission with full ERP data
    await page.evaluate(() => {
      window.testErpData = {
        lossOfC2: {
          action: 'RTL',
          timeout: 30,
          failsafe: 'Land immediately'
        },
        flyAway: {
          geofence: 'Active',
          maxRange: 5000,
          termination: 'Parachute deployment'
        },
        emergencyLanding: {
          sites: [
            { lat: 52.52, lon: 13.40, label: 'E1 - Stadium' }
          ],
          procedure: 'Descend to 50m AGL, circle site',
          minAgl: 10
        },
        groundNotification: {
          method: 'SMS + App',
          radius: 500,
          contacts: ['Police', 'ATC']
        },
        atsCoordination: {
          required: true,
          authority: 'DFS Berlin',
          frequency: '118.5 MHz'
        },
        safeArea: {
          center: { lat: 52.52, lon: 13.40 },
          radius: 1000
        }
      };
      
      window.updateErpPanel(window.testErpData);
    });
    
    await page.waitForTimeout(500);
    
    // Verify all 5 sections exist
    const sections = await page.locator('.erp-section').count();
    expect(sections).toBeGreaterThanOrEqual(5);
    
    // Verify section headings
    await expect(page.locator('h5:has-text("Loss of C2")')).toBeVisible();
    await expect(page.locator('h5:has-text("Fly-Away")')).toBeVisible();
    await expect(page.locator('h5:has-text("Emergency Landing")')).toBeVisible();
    await expect(page.locator('h5:has-text("Ground Notification")')).toBeVisible();
    await expect(page.locator('h5:has-text("ATS Coordination")')).toBeVisible();
  });

  test('ERP Panel: Loss of C2 details', async ({ page }) => {
    await page.evaluate(() => {
      window.updateErpPanel({
        lossOfC2: {
          action: 'Return to Launch',
          timeout: 45,
          failsafe: 'Auto-land at last known position'
        }
      });
    });
    
    await page.waitForTimeout(500);
    
    const lossC2Content = await page.locator('.erp-section:has(h5:has-text("Loss of C2"))').textContent();
    
    expect(lossC2Content).toContain('Return to Launch');
    expect(lossC2Content).toContain('45');
    expect(lossC2Content).toContain('Auto-land');
  });

  test('ERP Panel: Safe area rendered on map', async ({ page }) => {
    await page.evaluate(() => {
      window.updateErpPanel({
        safeArea: {
          center: { lat: 52.52, lon: 13.40 },
          radius: 1500
        }
      });
    });
    
    await page.waitForTimeout(1000);
    
    const safeAreaExists = await page.evaluate(() => {
      const map = (window as any).map2D;
      return map && map.getSource('erp-safe-area') !== undefined;
    });
    
    expect(safeAreaExists).toBe(true);
    
    // Verify legend entry
    await expect(page.locator('.legend-label:has-text("Safe Area")')).toBeVisible();
  });

  test('SORA Panel: Badge tooltips on hover', async ({ page }) => {
    await page.evaluate(() => {
      window.updateSoraBadges({
        initialGrc: 5,
        finalGrc: 3,
        initialArc: 'b',
        residualArc: 'a',
        sail: 'II'
      });
    });
    
    await page.waitForTimeout(500);
    
    // Check iGRC tooltip
    const igrcBadge = page.locator('#badge-igrc');
    await igrcBadge.hover();
    
    const igrcTitle = await igrcBadge.getAttribute('title');
    expect(igrcTitle).toContain('Initial GRC');
    expect(igrcTitle).toContain('Ground Risk Class');
    
    // Check fGRC tooltip
    const fgrcBadge = page.locator('#badge-fgrc');
    const fgrcTitle = await fgrcBadge.getAttribute('title');
    expect(fgrcTitle).toContain('Final GRC');
    expect(fgrcTitle).toContain('mitigations');
    
    // Check iARC tooltip
    const iarcBadge = page.locator('#badge-iarc');
    const iarcTitle = await iarcBadge.getAttribute('title');
    expect(iarcTitle).toContain('Initial ARC');
    expect(iarcTitle).toContain('Air Risk Class');
    
    // Check rARC tooltip
    const rarcBadge = page.locator('#badge-rarc');
    const rarcTitle = await rarcBadge.getAttribute('title');
    expect(rarcTitle).toContain('Residual ARC');
    expect(rarcTitle).toContain('tactical mitigations');
    
    // Check SAIL tooltip
    const sailBadge = page.locator('#badge-sail');
    const sailTitle = await sailBadge.getAttribute('title');
    expect(sailTitle).toContain('SAIL');
    expect(sailTitle).toContain('Assurance');
  });

  test('SORA Panel: Cursor changes to help on hover', async ({ page }) => {
    await page.evaluate(() => {
      window.updateSoraBadges({
        initialGrc: 4,
        finalGrc: 2,
        initialArc: 'b',
        residualArc: 'a',
        sail: 'I'
      });
    });
    
    await page.waitForTimeout(500);
    
    const igrcCursor = await page.locator('#badge-igrc').evaluate((el) => {
      return window.getComputedStyle(el).cursor;
    });
    
    expect(igrcCursor).toBe('help');
  });

  test('OSO Panel: Coverage percentage with green color (>80%)', async ({ page }) => {
    await page.evaluate(() => {
      window.updateOsoPanel({
        required: 20,
        covered: 18,
        missing: 2,
        missingOsos: [
          { code: 'OSO#01', label: 'Operational procedures designed, validated' },
          { code: 'OSO#08', label: 'Detect and avoid (DAA) system' }
        ]
      });
    });
    
    await page.waitForTimeout(500);
    
    // Check for percentage display
    const osoContent = await page.locator('#oso-panel-content').textContent();
    expect(osoContent).toContain('90.0%'); // (18/20)*100
    
    // Verify green color (>80%) - check data-hex attribute
    const percentageColorHex = await page.evaluate(() => {
      const elem = document.querySelector('#oso-coverage-color');
      return elem ? elem.getAttribute('data-hex') : null;
    });
    
    expect(percentageColorHex).toBe('10b981'); // Green hex
  });

  test('OSO Panel: Coverage percentage with yellow color (50-80%)', async ({ page }) => {
    await page.evaluate(() => {
      window.updateOsoPanel({
        required: 20,
        covered: 13,
        missing: 7,
        missingOsos: []
      });
    });
    
    await page.waitForTimeout(500);
    
    const osoContent = await page.locator('#oso-panel-content').textContent();
    expect(osoContent).toContain('65.0%'); // (13/20)*100
    
    // Verify yellow/amber color (50-80%) - check data-hex attribute
    const percentageColorHex = await page.evaluate(() => {
      const elem = document.querySelector('#oso-coverage-color');
      return elem ? elem.getAttribute('data-hex') : null;
    });
    
    expect(percentageColorHex).toBe('f59e0b'); // Yellow/Amber hex
  });

  test('OSO Panel: Coverage percentage with red color (<50%)', async ({ page }) => {
    await page.evaluate(() => {
      window.updateOsoPanel({
        required: 20,
        covered: 8,
        missing: 12,
        missingOsos: []
      });
    });
    
    await page.waitForTimeout(500);
    
    const osoContent = await page.locator('#oso-panel-content').textContent();
    expect(osoContent).toContain('40.0%'); // (8/20)*100
    
    // Verify red color (<50%) - check data-hex attribute
    const percentageColorHex = await page.evaluate(() => {
      const elem = document.querySelector('#oso-coverage-color');
      return elem ? elem.getAttribute('data-hex') : null;
    });
    
    expect(percentageColorHex).toBe('ef4444'); // Red hex
  });

  test('OSO Panel: Missing list shows first 5 OSOs', async ({ page }) => {
    await page.evaluate(() => {
      window.updateOsoPanel({
        required: 24,
        covered: 16,
        missing: 8,
        missingOsos: [
          { code: 'OSO#01', label: 'Operational procedures' },
          { code: 'OSO#03', label: 'UAS operator competency' },
          { code: 'OSO#06', label: 'C2 link performance' },
          { code: 'OSO#08', label: 'Detect and avoid' },
          { code: 'OSO#12', label: 'Automatic protection of flight envelope' },
          { code: 'OSO#15', label: 'Safe recovery from technical issue' },
          { code: 'OSO#18', label: 'Automatic emergency landing' },
          { code: 'OSO#21', label: 'Human factors assessment' }
        ]
      });
    });
    
    await page.waitForTimeout(500);
    
    const osoContent = await page.locator('#oso-panel-content').textContent();
    
    // Verify first 5 appear
    expect(osoContent).toContain('OSO#01');
    expect(osoContent).toContain('OSO#03');
    expect(osoContent).toContain('OSO#06');
    expect(osoContent).toContain('OSO#08');
    expect(osoContent).toContain('OSO#12');
    
    // Verify "more OSOs missing" message (actual text format from production code)
    expect(osoContent).toContain('+ 3 more');
  });

  test('OSO Panel: All covered message (100% coverage)', async ({ page }) => {
    await page.evaluate(() => {
      window.updateOsoPanel({
        required: 20,
        covered: 20,
        missing: 0,
        missingOsos: []
      });
    });
    
    await page.waitForTimeout(500);
    
    const osoContent = await page.locator('#oso-panel-content').textContent();
    
    expect(osoContent).toContain('100.0%');
    expect(osoContent).toContain('✅ All required OSOs are covered!');
  });
});
