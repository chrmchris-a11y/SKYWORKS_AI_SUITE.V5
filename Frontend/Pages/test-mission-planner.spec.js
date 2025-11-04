// Automated UI test for mission planner (drone specs, OSO tooltips, i18n)
// Uses vanilla JS and DOM assertions. Run in browser console or with a test runner like Jest + jsdom.

describe('Mission Planner UI', () => {
  beforeAll(() => {
    document.body.innerHTML = '';
    // Load mission.html into DOM (simulate)
    // For real browser, skip this step
  });

  it('should auto-update drone specs when model changes', () => {
    const select = document.getElementById('droneModel');
    const info = document.getElementById('droneInfo');
    if (!select || !info) throw new Error('Drone model or info missing');
    // Simulate selection
    select.value = select.options[1].value;
    select.dispatchEvent(new Event('change'));
    expect(info.style.display).toBe('block');
    expect(document.getElementById('droneManufacturer').textContent).not.toBe('-');
  });

  it('should show OSO tooltip on badge click', () => {
    const badge = document.querySelector('.oso-badge');
    badge.click();
    const tip = badge.querySelector('.oso-tooltip');
    expect(tip.style.display).toBe('block');
    document.body.click();
    expect(tip.style.display).toBe('none');
  });

  it('should display Greek translation for dropdowns/tooltips', () => {
    // Simulate Greek language
    window.setLanguage && window.setLanguage('el');
    const help = document.querySelector('.help-icon[data-i18n]');
    expect(help.title).toMatch(/Επιλέξτε/);
    const option = document.querySelector('option[data-i18n]');
    expect(option.textContent).not.toMatch(/Auto/);
  });
});
