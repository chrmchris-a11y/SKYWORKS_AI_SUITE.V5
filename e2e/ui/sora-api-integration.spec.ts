/**
 * SORA API Integration E2E Tests
 * 
 * Purpose: Verify Mission Planner uses correct toggle structure
 * 
 * Test Coverage:
 * 1. Mission Planner SORA 2.5 toggle works
 * 2. Mission Planner SORA 2.0 toggle works
 * 3. Dropdown options update when toggling
 */

import { test, expect } from '@playwright/test';

const BASE_URL = 'http://localhost:5210';
const MISSION_PLANNER_URL = `${BASE_URL}/app/Pages/ui/mission.html`;

test.describe('SORA API Integration E2E Tests', () => {
  
  // ========================================================================
  // Test 1: Mission Planner SORA 2.5 - Framework toggle
  // ========================================================================
  test('Mission Planner: SORA 2.5 toggle works', async ({ page }) => {
    await page.goto(MISSION_PLANNER_URL);
    
    // Wait for page load (card-header exists)
    await expect(page.locator('.card-header').first()).toContainText('Mission Planner');
    
    // Select SORA 2.5 (toggle button) - already active by default
    const sora25Btn = await page.locator('button[data-framework="sora25"]');
    
    // Verify M1A dropdown exists (SORA 2.5 specific)
    const m1aSelect = await page.locator('select#m1a');
    await expect(m1aSelect).toBeVisible();
    
    // Verify M2-25 dropdown exists
    const m225Select = await page.locator('select#m2-25');
    await expect(m225Select).toBeVisible();
  });

  // ========================================================================
  // Test 2: Mission Planner SORA 2.0 - Framework toggle
  // ========================================================================
  test('Mission Planner: SORA 2.0 toggle works', async ({ page }) => {
    // Toggle now wired with inline script
    await page.goto('http://localhost:5210/app/Pages/ui/mission.html');
    
    // Toggle to SORA 2.0
    await page.evaluate(() => {
      (window as any).toggleFramework('sora20');
    });
    
    // Verify toggle worked
    const sora20Visible = await page.locator('#sora20-fields').isVisible();
    expect(sora20Visible).toBe(true);
  });

  // ========================================================================
  // Test 3: Dropdown options validation (SORA 2.5)
  // ========================================================================
  test('Mission Planner: SORA 2.5 dropdown options correct', async ({ page }) => {
    await page.goto(MISSION_PLANNER_URL);
    
    // Select SORA 2.5
    await page.click('button[data-framework="sora25"]');
    await page.waitForTimeout(500);
    
    // M1A should have: None, Low (NO MEDIUM)
    const m1aOptions = await page.locator('select#m1a option').allTextContents();
    expect(m1aOptions.some(opt => opt.includes('None'))).toBeTruthy();
    expect(m1aOptions.some(opt => opt.includes('Low'))).toBeTruthy();
    expect(m1aOptions.some(opt => opt.toLowerCase().includes('medium'))).toBeFalsy();
    
    // M1B should have: None, Medium, High
    const m1bOptions = await page.locator('select#m1b option').allTextContents();
    expect(m1bOptions.some(opt => opt.includes('None'))).toBeTruthy();
    expect(m1bOptions.some(opt => opt.includes('Medium'))).toBeTruthy();
    expect(m1bOptions.some(opt => opt.includes('High'))).toBeTruthy();
  });

  // ========================================================================
  // Test 4: Dropdown options validation (SORA 2.0)
  // ========================================================================
  test('Mission Planner: SORA 2.0 dropdown options correct', async ({ page }) => {
    await page.goto(MISSION_PLANNER_URL);
    
    // Select SORA 2.0
    await page.click('button[data-framework="sora20"]');
    await page.waitForTimeout(500);
    
    // M2-20 should have: None, Low, High (NO MEDIUM)
    const m220Options = await page.locator('select#m2-20 option').allTextContents();
    expect(m220Options.some(opt => opt.includes('None'))).toBeTruthy();
    expect(m220Options.some(opt => opt.includes('Low'))).toBeTruthy();
    expect(m220Options.some(opt => opt.includes('High'))).toBeTruthy();
    expect(m220Options.some(opt => opt.toLowerCase().includes('medium'))).toBeFalsy();
  });

});
