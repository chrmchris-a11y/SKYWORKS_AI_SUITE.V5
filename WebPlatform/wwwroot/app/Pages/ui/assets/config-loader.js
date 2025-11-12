/**
 * Google Maps Config Loader
 * SORA-compliant map initialization for Skyworks EASA Mission Planner
 * 
 * STRICTLY: Google Maps JavaScript API only (NO OSM/Nominatim/MapLibre/Leaflet/Cesium)
 */

(function() {
  'use strict';

  const CONFIG_PATH = '/app/Pages/ui/config/maps.config.json';
  
  /**
   * Load Google Maps configuration and inject script dynamically
   */
  async function loadGoogleMapsConfig() {
    try {
      console.log('[Config Loader] Fetching maps.config.json...');
      
      const response = await fetch(CONFIG_PATH);
      if (!response.ok) {
        throw new Error(`Config not found: ${response.status} ${response.statusText}`);
      }
      
      const config = await response.json();
      console.log('[Config Loader] Config loaded:', { libraries: config.libraries, version: config.version });
      
      // Validate API key
      if (!config.googleMapsApiKey || config.googleMapsApiKey === '*****PLACEHOLDER*****') {
        showBlockingError('GOOGLE_MAPS_API_KEY_MISSING');
        return;
      }
      
      // Build Google Maps script URL
      const scriptUrl = `https://maps.googleapis.com/maps/api/js?` +
        `key=${config.googleMapsApiKey}` +
        `&libraries=${config.libraries}` +
        `&v=${config.version}` +
        `&callback=initGoogleMaps`;
      
      console.log('[Config Loader] Injecting Google Maps script...');
      
      // Inject script tag
      const script = document.createElement('script');
      script.src = scriptUrl;
      script.async = true;
      script.defer = true;
      script.onerror = () => {
        console.error('[Config Loader] Failed to load Google Maps script');
        showBlockingError('GOOGLE_MAPS_SCRIPT_LOAD_FAILED');
      };
      
      document.head.appendChild(script);
      
    } catch (error) {
      console.error('[Config Loader] Error:', error);
      showBlockingError('CONFIG_LOAD_ERROR', error.message);
    }
  }
  
  /**
   * Show blocking error panel (blocks UI until resolved)
   */
  function showBlockingError(errorType, details = '') {
    const errorMessages = {
      'GOOGLE_MAPS_API_KEY_MISSING': {
        title: 'Google Maps API Key Required',
        message: 'The Google Maps API key is missing or using placeholder value.',
        instructions: [
          '1. Go to <a href="https://console.cloud.google.com/" target="_blank">Google Cloud Console</a>',
          '2. Create/Select a project',
          '3. Enable APIs: Maps JavaScript API, Places API, Geocoding API',
          '4. Create API Key with HTTP referrer restrictions:',
          '   • http://localhost:5210/*',
          '5. Update <code>WebPlatform/wwwroot/app/Pages/ui/config/maps.config.json</code>',
          '6. Replace <code>*****PLACEHOLDER*****</code> with your API key',
          '7. Reload this page'
        ]
      },
      'GOOGLE_MAPS_SCRIPT_LOAD_FAILED': {
        title: 'Google Maps Script Load Failed',
        message: 'Failed to load Google Maps JavaScript API.',
        instructions: [
          'Check network connectivity',
          'Verify API key is valid and not expired',
          'Check browser console for specific error (RefererNotAllowedMapError, etc.)',
          'See README.md for detailed setup instructions'
        ]
      },
      'CONFIG_LOAD_ERROR': {
        title: 'Configuration Load Error',
        message: details || 'Failed to load maps.config.json',
        instructions: [
          'Verify <code>maps.config.json</code> exists in <code>ui/config/</code>',
          'Check JSON syntax (valid format)',
          'Ensure backend serves static files correctly'
        ]
      }
    };
    
    const error = errorMessages[errorType] || errorMessages['CONFIG_LOAD_ERROR'];
    
    // Create blocking overlay
    const overlay = document.createElement('div');
    overlay.id = 'google-maps-error-overlay';
    overlay.style.cssText = `
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.95);
      z-index: 999999;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    `;
    
    overlay.innerHTML = `
      <div style="max-width: 600px; padding: 40px; background: #1a1a1a; border-radius: 8px; border: 2px solid #ef4444;">
        <h2 style="color: #ef4444; margin: 0 0 16px 0; font-size: 24px;">⚠️ ${error.title}</h2>
        <p style="margin: 0 0 24px 0; font-size: 16px; color: #d1d5db;">${error.message}</p>
        <div style="background: #111; padding: 20px; border-radius: 4px; border-left: 4px solid #3b82f6;">
          <h3 style="margin: 0 0 12px 0; font-size: 14px; color: #60a5fa;">Setup Instructions:</h3>
          <ol style="margin: 0; padding-left: 20px; font-size: 14px; line-height: 1.8; color: #9ca3af;">
            ${error.instructions.map(instr => `<li>${instr}</li>`).join('')}
          </ol>
        </div>
        <p style="margin: 24px 0 0 0; font-size: 12px; color: #6b7280;">
          See <code style="background: #374151; padding: 2px 6px; border-radius: 3px;">README.md</code> 
          for detailed setup instructions and screenshots.
        </p>
      </div>
    `;
    
    document.body.appendChild(overlay);
  }
  
  /**
   * Healthcheck: Verify Google Maps is initialized
   */
  window.checkGoogleMapsHealth = function() {
    if (typeof google === 'undefined' || !google.maps) {
      console.error('[Healthcheck] Google Maps not loaded');
      return false;
    }
    
    if (!google.maps.Map || !google.maps.places || !google.maps.Geocoder) {
      console.error('[Healthcheck] Required Google Maps libraries missing');
      return false;
    }
    
    console.log('[Healthcheck] ✅ Google Maps ready');
    return true;
  };
  
  // Auto-load on DOMContentLoaded
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadGoogleMapsConfig);
  } else {
    loadGoogleMapsConfig();
  }
  
})();
