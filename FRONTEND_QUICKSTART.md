# Skyworks Frontend Quick Start

## Problem: CORS Errors & Missing Translations

When you open `mission.html` directly in a browser using `file://` protocol, you'll see:
- ❌ CORS policy blocks
- ❌ Translation files fail to load
- ❌ i18n shows "Missing key" warnings
- ❌ Dropdown/OSO labels show raw keys

## Solution: Run a Local Web Server

### Option 1: Use the Provided PowerShell Script (Recommended)

```powershell
# From the project root
.\start-frontend.ps1
```

Then open in your browser:
- **Mission Page**: http://localhost:8080/Pages/mission.html
- **Home**: http://localhost:8080/Pages/index.html

### Option 2: Python HTTP Server (Manual)

```powershell
cd Frontend
python -m http.server 8080
```

### Option 3: Node.js http-server (if you have Node installed)

```powershell
npm install -g http-server
cd Frontend
http-server -p 8080
```

### Option 4: VS Code Live Server Extension

1. Install "Live Server" extension in VS Code
2. Right-click `mission.html` → "Open with Live Server"

## Backend API (for drone data & SORA calculations)

The frontend expects the backend API at `http://localhost:5210`. To start it:

```powershell
cd Backend
dotnet run --project src/Skyworks.Api
```

## Quick Test Checklist

Once the server is running, verify:

1. ✅ Language switcher appears (EN/EL flags)
2. ✅ Switch language → all labels translate
3. ✅ Select a drone → specs auto-fill, KE updates
4. ✅ Population density → see helper text below dropdown
5. ✅ Click OSO badges → tooltip shows translated name/description
6. ✅ iGRC and ARC display update as you change inputs
7. ✅ Manual dimension/speed fields hidden when drone selected
8. ✅ Raw JSON hidden in collapsible section

## Troubleshooting

### Still seeing CORS errors?
- Make sure you're accessing via `http://localhost:8080`, **not** `file://`
- Check the browser console for the actual URL being fetched

### Translations not loading?
- Verify `Frontend/i18n/en.json` and `el.json` exist
- Open `http://localhost:8080/i18n/en.json` directly to test

### Backend API not responding?
- Start the backend: `cd Backend && dotnet run --project src/Skyworks.Api`
- Check `http://localhost:5210/health`
- Ensure no other process is using port 5210

### Drone dropdown empty?
- Backend must be running
- Check browser console for API errors
- Verify `http://localhost:5210/api/drones/dropdown` returns data

## Notes

- The frontend is **static HTML/JS** and works in any modern browser
- i18n translations are loaded dynamically via fetch (requires HTTP)
- Backend API provides drone catalog and SORA calculation endpoints
- All SORA 2.5 calculations (iGRC, ARC, GRC mitigations) are client-side for live feedback
