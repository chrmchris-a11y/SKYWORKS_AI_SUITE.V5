/**
 * Skyworks i18n Loader
 * Lightweight internationalization for Frontend
 * Supports 13 languages with localStorage persistence
 */

console.log('[i18n-loader.js] Script loaded');

class I18nLoader {
  constructor() {
    console.log('[i18n] Initializing I18nLoader');
    this.translations = {};
  this.fallbackLang = 'en';
  this.fallbackTranslations = {}; // cache of fallback language strings
    this.supportedLanguages = [
      'en', 'el', 'de', 'fr', 'es', 'it', 
      'ru', 'zh', 'pl', 'hr', 'sl', 'uk', 'cs'
    ];
    this.currentLang = this.detectLanguage();
    console.log('[i18n] Detected language:', this.currentLang);
  }

  /**
   * Detect user's preferred language
   * Priority: localStorage > navigator.language > fallback (en)
   */
  detectLanguage() {
    // 1. Check localStorage
    const stored = localStorage.getItem('skyworks_lang');
    if (stored && this.isSupported(stored)) {
      return stored;
    }

    // 2. Check browser language
    const browserLang = navigator.language || navigator.userLanguage;
    if (browserLang) {
      const langCode = browserLang.split('-')[0]; // 'el-GR' -> 'el'
      if (this.isSupported(langCode)) {
        return langCode;
      }
    }

    // 3. Fallback to English
    return this.fallbackLang;
  }

  /**
   * Check if language is supported
   */
  isSupported(lang) {
    return lang && this.supportedLanguages.includes(lang);
  }

  /**
   * Load translation file for a language
   */
  async load(lang = this.currentLang) {
    if (!this.isSupported(lang)) {
      console.warn(`[i18n] Language "${lang}" not supported, using fallback "${this.fallbackLang}"`);
      lang = this.fallbackLang;
    }

    try {
      // Try multiple base paths for robustness across serving setups
      const bases = [
        `/i18n`,
        `../i18n`,
        `/Frontend/i18n`,
        `${window.location.pathname.replace(/\/[^/]*$/, '')}/../i18n`,
        `/app/i18n`,
      ];
      let lastError = null;
      for (const base of bases) {
        const url = `${base}/${lang}.json`;
        try {
          const response = await fetch(url);
          if (!response.ok) throw new Error(`HTTP ${response.status}`);
          this.translations = await response.json();
          this.currentLang = lang;
          localStorage.setItem('skyworks_lang', lang);
          document.documentElement.lang = lang; // Update HTML lang attribute
          console.log(`[i18n] Loaded language: ${lang} from ${url}`);
          // Best-effort load of fallback translations for missing keys
          if (lang !== this.fallbackLang) {
            await this.loadFallback(bases);
          } else {
            this.fallbackTranslations = this.translations;
          }
          return this.translations;
        } catch (err) {
          lastError = err;
          console.warn(`[i18n] Failed to load from ${url}:`, err.message);
          continue;
        }
      }
      throw lastError || new Error('No translation sources available');
    } catch (error) {
      console.error(`[i18n] Failed to load ${lang}.json:`, error);
      // If failed and not already trying fallback, try fallback
      if (lang !== this.fallbackLang) {
        console.log(`[i18n] Retrying with fallback language: ${this.fallbackLang}`);
        return this.load(this.fallbackLang);
      }
      // As a last resort, continue with empty translations so the UI (incl. language switcher) still renders
      this.translations = {};
      this.currentLang = this.fallbackLang;
      document.documentElement.lang = this.fallbackLang;
      console.warn('[i18n] Proceeding with empty translations.');
      return this.translations;
    }
  }

  /**
   * Load fallback language (en) using provided bases. Silent on failure.
   */
  async loadFallback(bases) {
    for (const base of bases) {
      const url = `${base}/${this.fallbackLang}.json`;
      try {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        this.fallbackTranslations = await response.json();
        console.log(`[i18n] Fallback '${this.fallbackLang}' loaded from ${url}`);
        return;
      } catch (err) {
        // try next base silently
      }
    }
    console.warn('[i18n] Could not load fallback translations.');
  }

  /**
   * Get translation by key path (e.g., "missionPlanner.title")
   * @param {string} keyPath - Dot-notation path to translation
   * @param {object} params - Optional parameters for interpolation
   * @returns {string} Translated text or key if not found
   */
  t(keyPath, params = {}) {
    const keys = keyPath.split('.');

    // Resolve a key path against a dictionary
    const resolve = (dict) => {
      let val = dict;
      for (const k of keys) {
        if (val && typeof val === 'object' && k in val) {
          val = val[k];
        } else {
          return undefined;
        }
      }
      return typeof val === 'string' ? val : undefined;
    };

    // 1) Try current language
    let value = resolve(this.translations);
    // 2) Fallback to English if missing
    if (value === undefined && this.fallbackTranslations) {
      value = resolve(this.fallbackTranslations);
    }
    // 3) Humanize key if still missing
    if (value === undefined) {
      console.warn(`[i18n] Missing key (all langs): ${keyPath}`);
      const last = keys[keys.length - 1] || keyPath;
      value = last.replace(/([A-Z])/g, ' $1').replace(/[._-]/g, ' ').trim();
    }

    // If value is an object, something went wrong
    // value is now a string

    // Simple interpolation: replace {{param}} with values
    let result = value;
    for (const [param, paramValue] of Object.entries(params)) {
      result = result.replace(new RegExp(`{{${param}}}`, 'g'), paramValue);
    }

    return result;
  }

  /**
   * Change language and reload translations
   */
  async setLanguage(lang) {
    if (!this.isSupported(lang)) {
      console.error(`[i18n] Unsupported language: ${lang}`);
      return false;
    }

    await this.load(lang);
    
    // Dispatch custom event for UI updates
    window.dispatchEvent(new CustomEvent('languageChanged', { 
      detail: { lang, translations: this.translations } 
    }));

    return true;
  }

  /**
   * Get current language code
   */
  getCurrentLanguage() {
    return this.currentLang;
  }

  /**
   * Get all supported languages
   */
  getSupportedLanguages() {
    return this.supportedLanguages;
  }

  /**
   * Translate all elements with data-i18n attribute
   * Usage: <h1 data-i18n="missionPlanner.title"></h1>
   */
  translatePage() {
    const elements = document.querySelectorAll('[data-i18n]');
    elements.forEach(el => {
      const key = el.getAttribute('data-i18n');
      
      // Special handling for <option> elements in operationCategory dropdown
      if (el.tagName === 'OPTION' && el.closest('#operationCategory')) {
        const categoryValue = el.value;
        const translation = this.translations.missionPlanner?.category?.[categoryValue];
        if (translation) {
          el.textContent = translation;
          return;
        }
      }
      
      const translation = this.t(key);
      
      // Check if element has data-i18n-attr for attribute translation
      const attr = el.getAttribute('data-i18n-attr');
      if (attr) {
        el.setAttribute(attr, translation);
      } else {
        // For labels with child inputs/selects, only replace the text node
        // without destroying child elements
        if (el.tagName === 'LABEL' && el.children.length > 0) {
          // Replace only the first text node (label text before the input)
          const textNode = Array.from(el.childNodes).find(node => node.nodeType === Node.TEXT_NODE);
          if (textNode) {
            textNode.textContent = translation;
          }
        } else {
          // For elements without children, safe to use textContent
          el.textContent = translation;
        }
      }
    });
  }

  /**
   * Translate placeholder attributes
   * Usage: <input data-i18n-placeholder="missionPlanner.mission.missionIdPlaceholder" />
   */
  translatePlaceholders() {
    const elements = document.querySelectorAll('[data-i18n-placeholder]');
    elements.forEach(el => {
      const key = el.getAttribute('data-i18n-placeholder');
      el.placeholder = this.t(key);
    });
  }

  /**
   * Translate all elements on the page
   */
  translateAll() {
    this.translatePage();
    this.translatePlaceholders();
  }
}

// Create global instance
const i18n = new I18nLoader();
window.i18n = i18n; // Make it globally accessible
console.log('[i18n] Global instance created and attached to window');

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
  console.log('[i18n] Waiting for DOMContentLoaded');
  document.addEventListener('DOMContentLoaded', async () => {
    console.log('[i18n] DOMContentLoaded fired, loading translations');
    await i18n.load();
    i18n.translateAll();
    // Dispatch event to signal i18n is ready
    window.dispatchEvent(new CustomEvent('i18nReady', { detail: { language: i18n.getCurrentLanguage() } }));
    console.log('[i18n] Ready event dispatched');
  });
} else {
  // DOM already loaded
  console.log('[i18n] DOM already loaded, loading immediately');
  i18n.load().then(() => {
    i18n.translateAll();
    // Dispatch event to signal i18n is ready
    window.dispatchEvent(new CustomEvent('i18nReady', { detail: { language: i18n.getCurrentLanguage() } }));
    console.log('[i18n] Ready event dispatched');
  });
}

// Export for module usage (if needed)
if (typeof module !== 'undefined' && module.exports) {
  module.exports = i18n;
}
