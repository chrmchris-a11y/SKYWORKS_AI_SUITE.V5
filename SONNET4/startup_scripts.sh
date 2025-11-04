#!/bin/bash

# ============================================================================
# SORA 2.0 Backend Startup Script
# This script starts both Python and .NET services
# ============================================================================

echo "========================================================================"
echo "SORA 2.0 Backend Services Startup"
echo "========================================================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# ============================================================================
# STEP 1: Check Prerequisites
# ============================================================================

echo "Step 1: Checking prerequisites..."
echo ""

# Check Python
if command -v python3 &> /dev/null; then
    PYTHON_CMD=python3
    echo -e "${GREEN}✓${NC} Python 3 found: $(python3 --version)"
elif command -v python &> /dev/null; then
    PYTHON_CMD=python
    echo -e "${GREEN}✓${NC} Python found: $(python --version)"
else
    echo -e "${RED}✗${NC} Python not found. Please install Python 3.8+"
    exit 1
fi

# Check pip
if command -v pip3 &> /dev/null; then
    PIP_CMD=pip3
    echo -e "${GREEN}✓${NC} pip3 found"
elif command -v pip &> /dev/null; then
    PIP_CMD=pip
    echo -e "${GREEN}✓${NC} pip found"
else
    echo -e "${RED}✗${NC} pip not found. Please install pip"
    exit 1
fi

# Check .NET
if command -v dotnet &> /dev/null; then
    echo -e "${GREEN}✓${NC} .NET found: $(dotnet --version)"
else
    echo -e "${RED}✗${NC} .NET not found. Please install .NET 8 SDK"
    echo "   Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

echo ""
echo -e "${GREEN}All prerequisites satisfied!${NC}"
echo ""

# ============================================================================
# STEP 2: Setup Python Backend
# ============================================================================

echo "Step 2: Setting up Python backend..."
echo ""

# Navigate to Python backend
if [ ! -d "Backend_Python" ]; then
    echo -e "${RED}✗${NC} Backend_Python folder not found"
    echo "   Please create it and add main.py and requirements.txt"
    exit 1
fi

cd Backend_Python

# Check if main.py exists
if [ ! -f "main.py" ]; then
    echo -e "${RED}✗${NC} main.py not found in Backend_Python/"
    echo "   Please create main.py from the provided artifact"
    exit 1
fi

# Check if requirements.txt exists
if [ ! -f "requirements.txt" ]; then
    echo -e "${YELLOW}!${NC} requirements.txt not found, creating..."
    cat > requirements.txt << EOF
fastapi==0.104.1
uvicorn[standard]==0.24.0
pydantic==2.5.0
httpx==0.25.1
pytest==7.4.3
pytest-asyncio==0.21.1
python-multipart==0.0.6
EOF
fi

# Install Python dependencies
echo "Installing Python dependencies..."
$PIP_CMD install -r requirements.txt > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓${NC} Python dependencies installed"
else
    echo -e "${RED}✗${NC} Failed to install Python dependencies"
    exit 1
fi

cd ..
echo ""

# ============================================================================
# STEP 3: Setup .NET Backend
# ============================================================================

echo "Step 3: Setting up .NET backend..."
echo ""

# Navigate to .NET backend
if [ ! -d "Backend/src/Skyworks.Api" ]; then
    echo -e "${RED}✗${NC} Backend/src/Skyworks.Api folder not found"
    echo "   Please create the folder structure and add the .NET files"
    exit 1
fi

cd Backend/src/Skyworks.Api

# Check if .csproj exists
if [ ! -f "Skyworks.Api.csproj" ]; then
    echo -e "${RED}✗${NC} Skyworks.Api.csproj not found"
    echo "   Please create the project file from the provided artifact"
    exit 1
fi

# Restore .NET packages
echo "Restoring .NET packages..."
dotnet restore > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓${NC} .NET packages restored"
else
    echo -e "${RED}✗${NC} Failed to restore .NET packages"
    exit 1
fi

# Build .NET project
echo "Building .NET project..."
dotnet build > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓${NC} .NET project built successfully"
else
    echo -e "${RED}✗${NC} Failed to build .NET project"
    exit 1
fi

cd ../../..
echo ""

# ============================================================================
# STEP 4: Start Services
# ============================================================================

echo "Step 4: Starting services..."
echo ""
echo "========================================================================"
echo "Starting Python FastAPI (Port 8001)..."
echo "========================================================================"
echo ""

# Start Python service in background
cd Backend_Python
$PYTHON_CMD -m uvicorn main:app --port 8001 --reload &
PYTHON_PID=$!
cd ..

# Wait for Python service to start
sleep 3

# Check if Python service is running
if curl -s http://localhost:8001/health > /dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} Python service started successfully (PID: $PYTHON_PID)"
    echo "   Health: http://localhost:8001/health"
    echo "   Docs:   http://localhost:8001/docs"
else
    echo -e "${RED}✗${NC} Python service failed to start"
    kill $PYTHON_PID 2>/dev/null
    exit 1
fi

echo ""
echo "========================================================================"
echo "Starting .NET Core API (Port 5210)..."
echo "========================================================================"
echo ""

# Start .NET service in background
cd Backend/src/Skyworks.Api
dotnet run &
DOTNET_PID=$!
cd ../../..

# Wait for .NET service to start
sleep 5

# Check if .NET service is running
if curl -s http://localhost:5210/api/sora/health > /dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} .NET service started successfully (PID: $DOTNET_PID)"
    echo "   Health:  http://localhost:5210/api/sora/health"
    echo "   Swagger: http://localhost:5210/swagger"
else
    echo -e "${RED}✗${NC} .NET service failed to start"
    kill $PYTHON_PID 2>/dev/null
    kill $DOTNET_PID 2>/dev/null
    exit 1
fi

echo ""
echo "========================================================================"
echo -e "${GREEN}✓ ALL SERVICES RUNNING SUCCESSFULLY!${NC}"
echo "========================================================================"
echo ""
echo "Service Status:"
echo "  • Python FastAPI: http://localhost:8001 (PID: $PYTHON_PID)"
echo "  • .NET Core API:  http://localhost:5210 (PID: $DOTNET_PID)"
echo ""
echo "API Documentation:"
echo "  • Python Docs:    http://localhost:8001/docs"
echo "  • .NET Swagger:   http://localhost:5210/swagger"
echo ""
echo "Health Checks:"
echo "  • Python Health:  http://localhost:8001/health"
echo "  • .NET Health:    http://localhost:5210/api/sora/health"
echo ""
echo "To stop services:"
echo "  kill $PYTHON_PID $DOTNET_PID"
echo ""
echo "Or press Ctrl+C to stop this script and all services"
echo "========================================================================"
echo ""

# Save PIDs to file for easy cleanup
echo $PYTHON_PID > .sora_python.pid
echo $DOTNET_PID > .sora_dotnet.pid

# Keep script running and forward signals
trap "echo ''; echo 'Stopping services...'; kill $PYTHON_PID $DOTNET_PID 2>/dev/null; rm .sora_*.pid 2>/dev/null; exit" INT TERM

# Wait for processes
wait
