/**
 * E2E Test: Mission Planner SORA Version Toggle
 * Verifies that toggling between SORA 2.5 and 2.0 shows/hides correct fieldsets
 */

import { test, expect } from '@playwright/test';

test.describe('Mission Planner - SORA Version Toggle', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/mission.html');
  });

  test('should load mission.html successfully', async ({ page }) => {
    await expect(page).toHaveTitle(/Mission Planner/);
    await expect(page.locator('.card-header').first()).toContainText('Mission Planner');
  });

  test('should default to SORA 2.5', async ({ page }) => {
    // SORA 2.5 toggle button should be active
    const sora25Btn = page.locator('[data-version="2.5"]');
    await expect(sora25Btn).toHaveClass(/active/);

    // SORA 2.5 fields should be visible
    const sora25Fields = page.locator('#sora25-fields');
    await expect(sora25Fields).toBeVisible();

    // SORA 2.0 fields should be hidden
    const sora20Fields = page.locator('#sora20-fields');
    await expect(sora20Fields).toBeHidden();
  });

  test('should toggle to SORA 2.0 and show correct fields', async ({ page }) => {
    // Click SORA 2.0 button
    const sora20Btn = page.locator('[data-version="2.0"]');
    await sora20Btn.click();

    // SORA 2.0 button should be active
    await expect(sora20Btn).toHaveClass(/active/);

    // SORA 2.0 fields should be visible
    const sora20Fields = page.locator('#sora20-fields');
    await expect(sora20Fields).toBeVisible();

    // SORA 2.5 fields should be hidden
    const sora25Fields = page.locator('#sora25-fields');
    await expect(sora25Fields).toBeHidden();
  });

  test('should toggle back to SORA 2.5', async ({ page }) => {
    // Toggle to 2.0
    await page.locator('[data-version="2.0"]').click();
    
    // Toggle back to 2.5
    const sora25Btn = page.locator('[data-version="2.5"]');
    await sora25Btn.click();

    // Verify 2.5 is active and visible
    await expect(sora25Btn).toHaveClass(/active/);
    await expect(page.locator('#sora25-fields')).toBeVisible();
    await expect(page.locator('#sora20-fields')).toBeHidden();
  });

  test('should verify SORA 2.5 dropdown options', async ({ page }) => {
    // M1A should have None, Low (NO MEDIUM)
    const m1a = page.locator('#m1a');
    const m1aOptions = await m1a.locator('option').allTextContents();
    expect(m1aOptions).toEqual(['None', 'Low']);

    // M1B should have None, Medium, High
    const m1b = page.locator('#m1b');
    const m1bOptions = await m1b.locator('option').allTextContents();
    expect(m1bOptions).toEqual(['None', 'Medium (-1 credit)', 'High (-2 credits)']);

    // M2 2.5 should have None, Low, Medium, High
    const m2 = page.locator('#m2-25');
    const m2Options = await m2.locator('option').allTextContents();
    expect(m2Options).toContain('Low');
    expect(m2Options).toContain('Medium');
    expect(m2Options).toContain('High');
  });

  test('should verify SORA 2.0 dropdown options', async ({ page }) => {
    // Toggle to 2.0
    await page.locator('[data-version="2.0"]').click();

    // M2 2.0 should have None, Low, High (NO MEDIUM)
    const m2 = page.locator('#m2-20');
    const m2Options = await m2.locator('option').allTextContents();
    expect(m2Options).toEqual(['None', 'Low', 'High']);
    expect(m2Options).not.toContain('Medium');

    // M3 should have None, Adequate, Validated
    const m3 = page.locator('#m3-20');
    const m3Options = await m3.locator('option').allTextContents();
    expect(m3Options).toContain('None');
    expect(m3Options).toContain('Adequate');
    expect(m3Options).toContain('Validated');
  });
});
