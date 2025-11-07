/**
 * E2E Test: GRC 2.5 Validation - Reject M1A=Medium
 * Verifies that UI validation rejects M1A="Medium" (not allowed per Annex B Tables 2-3)
 */

import { test, expect } from '@playwright/test';

test.describe('GRC 2.5 Validation - M1A', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/mission.html');
  });

  test('M1A dropdown should not contain "Medium" option', async ({ page }) => {
    const m1a = page.locator('#m1a');
    const options = await m1a.locator('option').allTextContents();
    
    // Should only have None, Low
    expect(options).toEqual(['None', 'Low']);
    expect(options).not.toContain('Medium');
  });

  test('should show validation error if M1A=Medium is somehow injected', async ({ page }) => {
    // This test simulates client-side validation bypass
    // (e.g., if someone modifies DOM manually)
    
    // Inject invalid option via console
    await page.evaluate(() => {
      const m1a = document.getElementById('m1a') as HTMLSelectElement;
      const option = document.createElement('option');
      option.value = 'Medium';
      option.text = 'Medium (INVALID)';
      m1a.appendChild(option);
      m1a.value = 'Medium';
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
    await page.selectOption('#m1b', 'None');
    await page.selectOption('#m1c', 'None');
    await page.selectOption('#m2-25', 'None');

    // Submit form
    await page.click('button[type="submit"]');

    // Wait for validation console
    await page.waitForTimeout(500);

    // Check console for error
    const consoleText = await page.locator('#validation-console').textContent();
    expect(consoleText).toContain('Invalid M1A');
    expect(consoleText).toContain('Allowed: None, Low');
  });

  test('should accept valid M1A values (None, Low)', async ({ page }) => {
    // Fill form with valid M1A=Low
    await page.selectOption('#operation-type', 'VLOS');
    await page.selectOption('#airspace-class', 'G');
    await page.fill('#max-height', '60');
    await page.selectOption('#typicality', 'Typical');
    await page.selectOption('#uspace', 'No');
    await page.selectOption('#traffic-density', 'Empirical');
    await page.selectOption('#airspace-containment', 'None');
    await page.selectOption('#aec', 'AEC_9');
    await page.selectOption('#m1a', 'Low');
    await page.selectOption('#m1b', 'None');
    await page.selectOption('#m1c', 'None');
    await page.selectOption('#m2-25', 'None');

    // Submit form
    await page.click('button[type="submit"]');

    // Wait for response
    await page.waitForTimeout(1000);

    // Check console - should not show M1A error
    const consoleText = await page.locator('#validation-console').textContent();
    expect(consoleText).not.toContain('Invalid M1A');
  });
});
