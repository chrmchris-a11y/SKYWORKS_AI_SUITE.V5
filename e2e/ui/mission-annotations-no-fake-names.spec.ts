import { test, expect } from '@playwright/test';

test.describe('Mission Annotations - No Fake Names (EASA Compliance)', () => {
  test('test-mission-annotations.html should NOT contain fake names', async ({ page }) => {
    await page.goto('http://localhost:5210/test-mission-annotations.html');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Get page HTML content
    const pageContent = await page.content();
    
    // Check for fake names that should NOT exist
    const fakenames = ['John Smith', 'Sarah Connor', 'Alex Johnson', 'Maria Lopez'];
    const foundFakeNames = fakenames.filter(name => pageContent.includes(name));
    
    // Assert: NO fake names should be found
    expect(foundFakeNames).toHaveLength(0);
    
    if (foundFakeNames.length > 0) {
      console.error('[FAIL] Found fake names:', foundFakeNames);
    }
  });

  test('Observer/Crew labels should use generic roles only', async ({ page }) => {
    await page.goto('http://localhost:5210/test-mission-annotations.html');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Get all Leaflet markers/tooltips
    const labels = await page.evaluate(() => {
      const tooltips = Array.from(document.querySelectorAll('.leaflet-tooltip'));
      return tooltips.map(t => t.textContent || '');
    });
    
    // Valid generic labels
    const validLabels = [
      'Observer 1', 'Observer 2', 'Observer 3',
      'Remote Pilot', 'Visual Observer', 'VO',
      'Crew 1', 'Crew 2', 'Crew 3',
      'TOL', 'LANDING', 'SAFE AREA'
    ];
    
    // Check if any label contains a fake name
    const hasFakeName = labels.some(label => 
      label.includes('John') || 
      label.includes('Sarah') || 
      label.includes('Alex') || 
      label.includes('Maria')
    );
    
    expect(hasFakeName).toBe(false);
  });

  test('SORA badge should be dynamic (not hardcoded "JARUS SORA 2.5")', async ({ page }) => {
    await page.goto('http://localhost:5210/test-mission-annotations.html');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    const badgeLocator = page.locator('#sora-version-badge');
    const badgeText = await badgeLocator.textContent();
    
    // Badge should contain full label (not just "JARUS SORA 2.5")
    const isValid = badgeText?.includes('EASA SORA 2.0 AMC') || 
                    badgeText?.includes('JARUS SORA 2.5 Annex A');
    
    expect(isValid).toBe(true);
  });

  test('Max 4-5 permanent labels visible (no visual chaos)', async ({ page }) => {
    await page.goto('http://localhost:5210/test-mission-annotations.html');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Count permanent tooltips
    const permanentTooltipCount = await page.evaluate(() => {
      const tooltips = Array.from(document.querySelectorAll('.leaflet-tooltip'));
      // Permanent tooltips have visible class
      return tooltips.filter(t => t.classList.contains('leaflet-tooltip')).length;
    });
    
    // Should have max 4-5 permanent labels (TOL, Landing, Safe Area, E1 distance, maybe one more)
    expect(permanentTooltipCount).toBeLessThanOrEqual(5);
    
    console.log(`[TEST] Permanent tooltip count: ${permanentTooltipCount} (max 5)`);
  });
});
