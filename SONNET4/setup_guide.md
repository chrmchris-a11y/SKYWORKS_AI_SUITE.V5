# Complete Setup Guide: SORA 2.0 Backend M2 Mitigation Fix

## 🎯 What This Fix Does

This fix corrects the M2 (Impact Reduction) mitigation level from the incorrect "Medium" value to the correct "Low" value according to **JAR_doc_06 Table 3** (JARUS SORA 2.0 AMC).

**Before (❌ Wrong):**
- None (0)
- Medium (-1) ← **Incorrect!**
- High (-2)

**After (✅ Correct):**
- None (0)
- Low (-1) ← **Correct per official JARUS spec**
- High (-2)

---

## 📁 Project Structure

Your project should have this structure:

```
your-project/
├── Backend_Python/          # Python FastAPI (Port 8001)
│   ├── main.py             # ← You'll create/replace this
│   ├── requirements.txt    # ← You'll create/replace this
│   └── ...
│
├── Backend/                # .NET Core API (Port 5210)
│   ├── src/
│   │   └── Skyworks.Api/
│   │       ├── Controllers/
│   │       │   └── SoraController.cs  # ← You'll create/replace this
│   │       ├── Program.cs             # ← You'll create/replace this
│   │       └── Skyworks.Api.csproj
│   └── ...
│
└── app/                    # Frontend
    └── Pages/
        └── mission.html    # ← Already fixed (M2 dropdown)
```

---

## 🔧 Step-by-Step Setup Instructions

### **PART 1: Python FastAPI Backend (Port 8001)**

#### 1.1 Navigate to Python Backend Folder

Open a terminal and navigate to your Python backend folder:

```bash
cd Backend_Python
```

If this folder doesn't exist, create it:

```bash
mkdir Backend_Python
cd Backend_Python
```

#### 1.2 Create/Replace `main.py`

Copy the **complete Python code** I provided in the artifact above into a file named `main.py` in your `Backend_Python` folder.

**On Windows:**
```powershell
# Copy the content from the artifact "Python FastAPI Backend - main.py"
# Save it as: Backend_Python/main.py
```

**On macOS/Linux:**
```bash
# Copy the content from the artifact "Python FastAPI Backend - main.py"
# Save it as: Backend_Python/main.py
```

#### 1.3 Create/Replace `requirements.txt`

Copy the **requirements.txt content** from the artifact into a file named `requirements.txt`:

```bash
# In Backend_Python/ folder, create requirements.txt with this content:
fastapi==0.104.1
uvicorn[standard]==0.24.0
pydantic==2.5.0
python-multipart==0.0.6
```

#### 1.4 Install Python Dependencies

```bash
# Make sure you're in Backend_Python/ folder
pip install -r requirements.txt
```

**If you get a "pip not found" error:**
```bash
# Try python3 and pip3 instead:
python3 -m pip install -r requirements.txt
```

#### 1.5 Start Python FastAPI Server

```bash
# Start the server on port 8001
python -m uvicorn main:app --port 8001 --reload
```

**Alternative (if the above doesn't work):**
```bash
python3 -m uvicorn main:app --port 8001 --reload
```

**You should see output like this:**
```
INFO:     Uvicorn running on http://0.0.0.0:8001 (Press CTRL+C to quit)
INFO:     Started reloader process
INFO:     Started server process
INFO:     Waiting for application startup.
================================================================================
SORA 2.0 GRC Calculator API Starting
Port: 8001
✅ FIXED: M2 mitigation levels corrected to None/Low/High
Reference: JAR_doc_06 Table 3 - JARUS SORA 2.0 AMC
================================================================================
INFO:     Application startup complete.
```

#### 1.6 Test Python Service is Running

Open a new terminal or browser and test:

```bash
# Test health endpoint
curl http://localhost:8001/health
```

**Or open in browser:**
```
http://localhost:8001/health
```

**Expected response:**
```json
{
  "status": "healthy",
  "service": "SORA 2.0 GRC Calculator",
  "port": 8001,
  "version": "2.0.0",
  "m2_levels": ["None (0)", "Low (-1)", "High (-2)"],
  "reference": "JAR_doc_06 Table 3"
}
```

✅ **If you see this, Python service is running correctly!**

**Keep this terminal open** - the server needs to keep running.

---

### **PART 2: .NET Core API Backend (Port 5210)**

#### 2.1 Navigate to .NET Backend Folder

Open a **NEW terminal** (keep Python running in the first one) and navigate to your .NET backend:

```bash
cd Backend/src/Skyworks.Api
```

**If your structure is different, find where your .csproj file is:**
```bash
# Search for the .csproj file
find . -name "*.csproj"
# or on Windows:
dir /s *.csproj
```

#### 2.2 Create/Update Project Files

You need to create or update these files:

**File 1: `Controllers/SoraController.cs`**
- Create folder: `Controllers/` (if it doesn't exist)
- Copy the **complete .NET Controller code** from the artifact above
- Save as: `Controllers/SoraController.cs`

**File 2: `Program.cs`**
- Copy the **complete .NET Program.cs code** from the artifact above
- Replace or create: `Program.cs` in the root of your Skyworks.Api project

#### 2.3 Verify/Update `.csproj` File

Your `Skyworks.Api.csproj` should include these packages:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="7.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
```

**If using .NET 8:**
```xml
<TargetFramework>net8.0</TargetFramework>
```

#### 2.4 Restore and Build .NET Project

```bash
# Make sure you're in the Skyworks.Api folder (where .csproj is)

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

#### 2.5 Start .NET API Server

```bash
# Run the API on port 5210
dotnet run
```

**You should see output like this:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5210
================================================================================
SORA 2.0 Orchestration API Starting
Port: 5210
✅ FIXED: M2 mitigation corrected to None/Low/High
Reference: JAR_doc_06 Table 3 - JARUS SORA 2.0 AMC
Python GRC Service: http://localhost:8001
Swagger UI: http://localhost:5210/swagger
================================================================================
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

#### 2.6 Test .NET Service is Running

Open a new terminal or browser and test:

```bash
# Test health endpoint
curl http://localhost:5210/api/sora/health
```

**Or open in browser:**
```
http://localhost:5210/api/sora/health
```

**Expected response:**
```json
{
  "status": "healthy",
  "service": "SORA Orchestration API",
  "port": 5210,
  "version": "2.0.0",
  "python_grc_service": "http://localhost:8001",
  "important_fix": "M2 mitigation corrected to None/Low/High per JAR_doc_06 Table 3"
}
```

✅ **If you see this, .NET service is running correctly!**

**Keep this terminal open too** - both services need to run simultaneously.

---

### **PART 3: Test the Complete Flow**

Now both services are running. Time to test the frontend!

#### 3.1 Open Your Frontend

Open your `mission.html` file in a browser:

```bash
# If using VS Code Live Server:
Right-click mission.html → Open with Live Server

# Or open directly:
file:///path/to/your/app/Pages/mission.html
```

#### 3.2 Fill Out SORA 2.0 Form

1. **Select SORA Category**: "SORA-2.0"
2. **Fill in GRC inputs:**
   - Initial GRC: `5` (example)
   - M1: `-1` (example)
   - **M2**: Select **"Low (-1)"** ← This is the fix!
   - M3: `0` (example)
3. **Fill in ARC inputs** (example values)
4. **Click**: "Execute SORA Assessment"

#### 3.3 Expected Result

**You should see:**
- ✅ **Status**: Success (no more "BadRequest" error)
- ✅ **Final GRC**: Calculated correctly (e.g., 5 + (-1) + (-1) + 0 = 3)
- ✅ **Final ARC**: Calculated
- ✅ **SAIL**: Determined (e.g., "II" or "III")

**In browser console (F12):**
```
[SORA Evaluation] Success!
Final GRC: 3
Final ARC: B
SAIL: II
```

#### 3.4 Verify the Fix

**Test that "Medium" is rejected:**
1. Open browser Developer Tools (F12)
2. Go to Console tab
3. Run this JavaScript:

```javascript
fetch('http://localhost:5210/api/sora/complete', {
  method: 'POST',
  headers: {'Content-Type': 'application/json'},
  body: JSON.stringify({
    category: 'SORA-2.0',
    grcInputs: {initialGrc: 5, m1: -1, m2: 'Medium', m3: 0},
    arcInputs: {initialArc: 'a', m4: 0, m5: 0, m6: 0}
  })
})
.then(r => r.json())
.then(console.log);
```

**Expected result:**
```json
{
  "error": "Invalid M2 level",
  "message": "M2 level 'Medium' is not valid for SORA 2.0. Expected: None, Low, or High...",
  "valid_levels": ["None", "Low", "High"]
}
```

✅ This confirms the fix is working!

---

## 🐛 Troubleshooting

### Problem 1: Python Service Won't Start

**Error:** `ModuleNotFoundError: No module named 'fastapi'`

**Solution:**
```bash
pip install fastapi uvicorn pydantic
# or
pip3 install fastapi uvicorn pydantic
```

---

### Problem 2: Port Already in Use

**Error:** `Address already in use` on port 8001 or 5210

**Solution:**

**On Windows:**
```powershell
# Find what's using the port
netstat -ano | findstr :8001
netstat -ano | findstr :5210

# Kill the process (replace PID with actual number)
taskkill /PID <PID> /F
```

**On macOS/Linux:**
```bash
# Find what's using the port
lsof -i :8001
lsof -i :5210

# Kill the process
kill -9 <PID>
```

---

### Problem 3: .NET Build Errors

**Error:** `The target framework 'net7.0' is not supported`

**Solution:**
1. Check your .NET version: `dotnet --version`
2. Update `.csproj` to match your version:
   ```xml
   <TargetFramework>net8.0</TargetFramework>
   ```

---

### Problem 4: CORS Errors in Browser

**Error:** `Access to fetch at 'http://localhost:5210' from origin '...' has been blocked by CORS policy`

**Solution:**
- Verify both `Program.cs` (lines 16-24) and `SoraController.cs` have CORS enabled
- The code I provided already includes this fix

---

### Problem 5: Services Start But Frontend Still Fails

**Checklist:**
1. ✅ Python service running? Check: `http://localhost:8001/health`
2. ✅ .NET service running? Check: `http://localhost:5210/api/sora/health`
3. ✅ Frontend M2 dropdown shows "Low" not "Medium"? 
4. ✅ Browser console shows which exact error?

**Get detailed logs:**
- Python terminal will show all incoming requests
- .NET terminal will show all incoming requests
- Browser console (F12) will show the exact error

---

## 📊 Testing Checklist

Use this to verify everything works:

- [ ] Python service starts on port 8001
- [ ] `/health` endpoint returns 200 OK
- [ ] .NET service starts on port 5210
- [ ] `/api/sora/health` endpoint returns 200 OK
- [ ] Frontend M2 dropdown shows "None/Low/High" (not "Medium")
- [ ] SORA evaluation with M2="Low" succeeds
- [ ] SORA evaluation with M2="Medium" returns 400 error
- [ ] Final GRC calculation is correct (manual verification)
- [ ] SAIL determination displays correctly

---

## 🎓 Understanding the Architecture

```
┌─────────────┐
│   Frontend  │  mission.html (Port: file:// or http://localhost)
│  (Browser)  │
└──────