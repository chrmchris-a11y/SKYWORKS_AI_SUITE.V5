# SKYWORKS_AI_SUITE.V5
UAS Risk Management Platform – SORA compliant

---

## 🗺️ Google Maps API Key Setup (REQUIRED)

**Skyworks uses STRICTLY Google Maps JavaScript API for all mapping features.**  
NO OSM/Nominatim/MapLibre/Leaflet/Cesium or other third-party providers.

### Step-by-Step Setup:

#### 1️⃣ Create Google Cloud Project
- Go to [Google Cloud Console](https://console.cloud.google.com/)
- Click **"Create Project"**
- Name: `Skyworks-EASA-Maps` (or your choice)
- Click **"Create"**

#### 2️⃣ Enable Required APIs
Navigate to **APIs & Services** → **Library**, then enable:
- ✅ **Maps JavaScript API**
- ✅ **Places API**
- ✅ **Geocoding API**
- ✅ **Geolocation API** (optional, for device location)
- ✅ **Elevation API** (optional, for terrain heights)

#### 3️⃣ Create API Key
- Go to **APIs & Services** → **Credentials**
- Click **"+ CREATE CREDENTIALS"** → **API key**
- Copy the generated key (save it securely!)

#### 4️⃣ Restrict API Key (IMPORTANT!)
Click **"EDIT API KEY"** (after creation):

**Application restrictions:**
- Select **"HTTP referrers (websites)"**
- Add referrers:
  ```
  http://localhost:5210/*
  http://127.0.0.1:5210/*
  ```
  _(For production: add your production domain)_

**API restrictions:**
- Select **"Restrict key"**
- Choose:
  - Maps JavaScript API
  - Places API
  - Geocoding API
  - (Optional: Geolocation API, Elevation API)

Click **"SAVE"**

#### 5️⃣ Add Key to Configuration
Edit: `WebPlatform/wwwroot/app/Pages/ui/config/maps.config.json`

```json
{
  "googleMapsApiKey": "YOUR_ACTUAL_API_KEY_HERE",
  "libraries": "places,geocoding,marker",
  "version": "weekly"
}
```

Replace `*****PLACEHOLDER*****` with your actual Google Maps API key.

#### 6️⃣ Verify Setup
1. Start Backend API: `dotnet run --project Backend/src/Skyworks.Api/Skyworks.Api.csproj --urls http://localhost:5210`
2. Open: `http://localhost:5210/app/Pages/ui/airspace-maps.html`
3. **Expected:** Google Maps loads successfully with Athens, Greece centered
4. **If error:** Check browser console for specific error codes (RefererNotAllowedMapError, etc.)

---

### 💰 Pricing (FREE Tier)
- **$200 free credit/month** (covers ~28,000 map loads)
- Development/testing is FREE for most use cases
- See [Google Maps Pricing](https://mapsplatform.google.com/pricing/)

### 🔒 Security Best Practices
- ✅ ALWAYS use HTTP referrer restrictions
- ✅ NEVER commit API keys to git (use `.gitignore` for `maps.config.json`)
- ✅ Monitor usage in Google Cloud Console
- ✅ Set billing alerts to avoid unexpected charges

---
