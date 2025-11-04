/**
 * Language Switcher Component
 * Dropdown selector for changing UI language
 */

console.log('[language-switcher.js] Script loaded');

class LanguageSwitcher {
  constructor(containerId = 'languageSwitcher') {
    this.containerId = containerId;
    this.languages = {
      en: { name: 'English', flag: '🇬🇧' },
      el: { name: 'Ελληνικά', flag: '🇬🇷' },
      de: { name: 'Deutsch', flag: '🇩🇪' },
      fr: { name: 'Français', flag: '🇫🇷' },
      es: { name: 'Español', flag: '🇪🇸' },
      it: { name: 'Italiano', flag: '🇮🇹' },
      ru: { name: 'Русский', flag: '🇷🇺' },
      zh: { name: '中文', flag: '🇨🇳' },
      pl: { name: 'Polski', flag: '🇵🇱' },
      hr: { name: 'Hrvatski', flag: '🇭🇷' },
      sl: { name: 'Slovenščina', flag: '🇸🇮' },
      uk: { name: 'Українська', flag: '🇺🇦' },
      cs: { name: 'Čeština', flag: '🇨🇿' }
    };
  }

  /**
   * Render language switcher dropdown
   */
  render() {
    const container = document.getElementById(this.containerId);
    if (!container) {
      console.error(`[LanguageSwitcher] Container #${this.containerId} not found`);
      return;
    }

    // Check if i18n is available
    if (typeof window.i18n === 'undefined') {
      console.error('[LanguageSwitcher] i18n not loaded yet');
      return;
    }

    const currentLang = window.i18n.getCurrentLanguage();
    const currentLangInfo = this.languages[currentLang];

    const html = `
      <div class="language-switcher">
        <button class="lang-button" id="langButton">
          <span class="lang-flag">${currentLangInfo.flag}</span>
          <span class="lang-name">${currentLangInfo.name}</span>
          <span class="lang-arrow">▼</span>
        </button>
        <div class="lang-dropdown" id="langDropdown" style="display: none;">
          ${this.renderLanguageOptions()}
        </div>
      </div>
    `;

    container.innerHTML = html;
    this.attachEventListeners();
    this.injectStyles();
  }

  /**
   * Render language options dropdown
   */
  renderLanguageOptions() {
    const currentLang = window.i18n.getCurrentLanguage();
    
    return Object.entries(this.languages)
      .map(([code, info]) => {
        const active = code === currentLang ? 'active' : '';
        return `
          <div class="lang-option ${active}" data-lang="${code}">
            <span class="lang-flag">${info.flag}</span>
            <span class="lang-name">${info.name}</span>
            ${code === currentLang ? '<span class="lang-check">✓</span>' : ''}
          </div>
        `;
      })
      .join('');
  }

  /**
   * Attach event listeners
   */
  attachEventListeners() {
    const button = document.getElementById('langButton');
    const dropdown = document.getElementById('langDropdown');

    if (!button || !dropdown) return;

    // Toggle dropdown
    button.addEventListener('click', (e) => {
      e.stopPropagation();
      const isVisible = dropdown.style.display === 'block';
      dropdown.style.display = isVisible ? 'none' : 'block';
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', () => {
      dropdown.style.display = 'none';
    });

    // Language selection
    dropdown.addEventListener('click', async (e) => {
      e.stopPropagation();
      const option = e.target.closest('.lang-option');
      if (!option) return;

      const lang = option.dataset.lang;
      if (lang) {
        await this.changeLanguage(lang);
        dropdown.style.display = 'none';
      }
    });

    // Listen for language changes from other sources
    window.addEventListener('languageChanged', () => {
      this.render();
    });
  }

  /**
   * Change language
   */
  async changeLanguage(lang) {
    try {
      await window.i18n.setLanguage(lang);
      window.i18n.translateAll();
      
      // Update button display
      const button = document.getElementById('langButton');
      const langInfo = this.languages[lang];
      if (button && langInfo) {
        button.innerHTML = `
          <span class="lang-flag">${langInfo.flag}</span>
          <span class="lang-name">${langInfo.name}</span>
          <span class="lang-arrow">▼</span>
        `;
      }

      console.log(`[LanguageSwitcher] Language changed to: ${lang}`);
    } catch (error) {
      console.error('[LanguageSwitcher] Failed to change language:', error);
    }
  }

  /**
   * Inject CSS styles
   */
  injectStyles() {
    if (document.getElementById('languageSwitcherStyles')) return;

    const style = document.createElement('style');
    style.id = 'languageSwitcherStyles';
    style.textContent = `
      .language-switcher {
        position: relative;
        display: inline-block;
      }

      .lang-button {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 8px 12px;
        background: white;
        border: 1px solid #ddd;
        border-radius: 6px;
        cursor: pointer;
        font-size: 14px;
        transition: all 0.2s;
      }

      .lang-button:hover {
        background: #f5f5f5;
        border-color: #999;
      }

      .lang-flag {
        font-size: 18px;
        line-height: 1;
      }

      .lang-name {
        font-weight: 500;
      }

      .lang-arrow {
        font-size: 10px;
        color: #666;
      }

      .lang-dropdown {
        position: absolute;
        top: 100%;
        right: 0;
        margin-top: 4px;
        background: white;
        border: 1px solid #ddd;
        border-radius: 6px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        min-width: 200px;
        max-height: 400px;
        overflow-y: auto;
        z-index: 1000;
      }

      .lang-option {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 12px;
        cursor: pointer;
        transition: background 0.2s;
      }

      .lang-option:hover {
        background: #f5f5f5;
      }

      .lang-option.active {
        background: #e3f2fd;
      }

      .lang-option .lang-name {
        flex: 1;
      }

      .lang-check {
        color: #0066cc;
        font-weight: bold;
      }

      @media (max-width: 768px) {
        .lang-button .lang-name {
          display: none;
        }
      }
    `;
    document.head.appendChild(style);
  }
}

// Auto-initialize when i18n is ready
console.log('[LanguageSwitcher] Setting up i18nReady listener');
window.addEventListener('i18nReady', () => {
  console.log('[LanguageSwitcher] i18nReady event received, rendering switcher');
  const switcher = new LanguageSwitcher();
  switcher.render();
});
