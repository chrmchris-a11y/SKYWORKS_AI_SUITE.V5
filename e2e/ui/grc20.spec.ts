/**
 * E2E Test: GRC 2.0 Validation - Reject M2=Medium
 * Verifies that UI validation rejects M2="Medium" (not allowed in SORA 2.0 AMC1 Art.11)
 */

import { test, expect } from '@playwright/test';

test.describe('GRC 2.0 Validation - M2', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/mission.html');
    
    // Toggle to SORA 2.0
    await page.click('[data-version="2.0"]');
  });

  test('M2 2.0 dropdown should not contain "Medium" option', async ({ page }) => {
    const m2 = page.locator('#m2-20');
    const options = await m2.locator('option').allTextContents();
    
    // Should only have None, Low, High (NO MEDIUM)
    expect(options).toEqual(['None', 'Low', 'High']);
    expect(options).not.toContain('Medium');
  });

  test('should show validation error if M2=Medium is somehow injected', async ({ page }) => {
    // Inject invalid option via console
    await page.evaluate(() => {
      const m2 = document.getElementById('m2-20') as HTMLSelectElement;
      const option = document.createElement('option');
      option.value = 'Medium';
      option.text = 'Medium (INVALID)';
      m2.appendChild(option);
      m2.value = 'Medium';
    });

    // Fill other required fields
    await page.selectOption('#operation-type', 'VLOS');
    await page.selectOption('#airspace-class', 'G');
    await page.fill('#max-height', '60');
    await page.selectOption('#typicality', 'Typical');
    await page.selectOption('#uspace', 'No');
    await page.selectOption('#traffic-density', 'Empirical');
    await page.selectOption('#airspace-containment', 'None');
    await page.selectOption('#aec', 'AEC_9');
    await page.selectOption('#m1-20', 'None');
    await page.selectOption('#m3-20', 'None');

    // Submit form
    await page.click('button[type="submit"]');

    // Wait for validation console
    await page.waitForTimeout(500);

    // Check console for error
    const consoleText = await page.locator('#validation-console').textContent();
    expect(consoleText).toContain('Invalid M2');
    expect(consoleText).toContain('NO MEDIUM in SORA 2.0');
  });

  test('should accept valid M2 values (None, Low, High)', async ({ page }) => {
    // Fill form with valid M2=High
    await page.selectOption('#operation-type', 'VLOS');
    await page.selectOption('#airspace-class', 'G');
    await page.fill('#max-height', '60');
    await page.selectOption('#typicality', 'Typical');
    await page.selectOption('#uspace', 'No');
    await page.selectOption('#traffic-density', 'Empirical');
    await page.selectOption('#airspace-containment', 'None');
    await page.selectOption('#aec', 'AEC_9');
    await page.selectOption('#m1-20', 'None');
    await page.selectOption('#m2-20', 'High');
    await page.selectOption('#m3-20', 'None');

    // Submit form
    await page.click('button[type="submit"]');

    // Wait for response
    await page.waitForTimeout(1000);

    // Check console - should not show M2 error
    const consoleText = await page.locator('#validation-console').textContent();
    expect(consoleText).not.toContain('Invalid M2');
  });

  test('M3 should use correct terminology (None/Adequate/Validated)', async ({ page }) => {
    const m3 = page.locator('#m3-20');
    const options = await m3.locator('option').allTextContents();
    
    // Should have official terminology
    expect(options).toContain('None (credit +1)');
    expect(options).toContain('Adequate (credit 0)');
    expect(options).toContain('Validated (credit -1)');
    
    // Should NOT have Low/Medium/High
    expect(options.join('')).not.toContain('Low');
    expect(options.join('')).not.toContain('Medium');
    expect(options.join('')).not.toContain('High');
  });
});
