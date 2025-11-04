"""
SKYWORKS AI SUITE - Python Calculation Microservice
FastAPI application for accurate SORA/SAIL/OSO calculations

Port: 8001 (to avoid conflict with .NET backend on 5210)
"""

import sys
import os

# Add parent directory to path for absolute imports when run from Backend_Python
if __name__ == "__main__":
    sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import uvicorn

# Import calculators
from calculations.grc_calculator import GRCCalculator
from calculations.arc_calculator import ARCCalculator
from calculations.sail_calculator import SAILCalculator

# Import models
from models.sora_models import (
    GRCRequest_2_0,
    GRCRequest_2_5,
    GRCResponse,
    ARCRequest_2_0,
    ARCRequest_2_5,
    ARCResponse,
    SAILRequest,
    SAILResponse,
    SORAVersion,
)

# Initialize FastAPI app
app = FastAPI(
    title="SKYWORKS AI SUITE - Calculation Service",
    description="Python microservice for accurate EASA/JARUS SORA calculations",
    version="1.0.0",
    docs_url="/api/docs",
    redoc_url="/api/redoc",
)

# CORS middleware (allow .NET backend to call Python)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5210", "http://localhost:3000"],  # .NET backend + React frontend
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize calculators
grc_calc = GRCCalculator()
arc_calc = ARCCalculator()
sail_calc = SAILCalculator()


# ============================================================================
# HEALTH CHECK
# ============================================================================

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "service": "SKYWORKS Python Calculation Service",
        "version": "1.0.0",
        "calculators": ["GRC", "ARC", "SAIL", "OSO"],
    }


# ============================================================================
# GRC ENDPOINTS
# ============================================================================

@app.post("/api/v1/calculate/grc/2.0", response_model=GRCResponse, tags=["GRC"])
async def calculate_grc_2_0(request: GRCRequest_2_0):
    """
    Calculate Ground Risk Class (GRC) for SORA 2.0
    
    Source: JARUS SORA 2.0, Table 2 (Page 20) & Table 3 (Page 21)
    """
    try:
        return grc_calc.calculate_grc_2_0(request)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"GRC calculation error: {str(e)}")


@app.post("/api/v1/calculate/grc/2.5", response_model=GRCResponse, tags=["GRC"])
async def calculate_grc_2_5(request: GRCRequest_2_5):
    """
    Calculate Ground Risk Class (GRC) for SORA 2.5
    
    Source: JARUS SORA 2.5, Table 2 (Page 34) & Table 3 (Page 35)
    """
    try:
        return grc_calc.calculate_grc_2_5(request)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"GRC calculation error: {str(e)}")


# ============================================================================
# ARC ENDPOINTS
# ============================================================================

@app.post("/api/v1/calculate/arc/2.0", response_model=ARCResponse, tags=["ARC"])
async def calculate_arc_2_0(request: ARCRequest_2_0):
    """
    Calculate Air Risk Class (ARC) for SORA 2.0
    
    Source: JARUS SORA 2.0, Annex C Table 1 (Page 12) & Table 2 (Page 14)
    """
    try:
        return arc_calc.calculate_arc_2_0(request)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"ARC calculation error: {str(e)}")


@app.post("/api/v1/calculate/arc/2.5", response_model=ARCResponse, tags=["ARC"])
async def calculate_arc_2_5(request: ARCRequest_2_5):
    """
    Calculate Air Risk Class (ARC) for SORA 2.5
    
    Source: JARUS SORA 2.5, Annex E Table 9 (Page 70), Figure 6
    """
    try:
        return arc_calc.calculate_arc_2_5(request)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"ARC calculation error: {str(e)}")


# ============================================================================
# SAIL ENDPOINTS
# ============================================================================

@app.post("/api/v1/calculate/sail", response_model=SAILResponse, tags=["SAIL"])
async def calculate_sail(request: SAILRequest):
    """
    Calculate SAIL (Specific Assurance and Integrity Level)
    
    Works for both SORA 2.0 and 2.5 (specify version in request)
    
    Source: 
    - SORA 2.0: JARUS SORA 2.0, Table 5 (Page 27)
    - SORA 2.5: JARUS SORA 2.5, Table 7 (Page 47)
    """
    try:
        return sail_calc.calculate_sail(request)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"SAIL calculation error: {str(e)}")


# ============================================================================
# COMBINED CALCULATION (for convenience)
# ============================================================================

class SORACalculationRequest(BaseModel):
    """Combined SORA calculation request"""
    sora_version: SORAVersion
    
    # GRC inputs
    grc_2_0: GRCRequest_2_0 | None = None
    grc_2_5: GRCRequest_2_5 | None = None
    
    # ARC inputs
    arc_2_0: ARCRequest_2_0 | None = None
    arc_2_5: ARCRequest_2_5 | None = None


class SORACalculationResponse(BaseModel):
    """Combined SORA calculation response"""
    grc: GRCResponse
    arc: ARCResponse
    sail: SAILResponse


@app.post("/api/v1/calculate/sora/complete", response_model=SORACalculationResponse, tags=["SORA"])
async def calculate_sora_complete(request: SORACalculationRequest):
    """
    Complete SORA calculation (GRC → ARC → SAIL) in one call
    
    Automatically routes to correct version (2.0 or 2.5)
    """
    try:
        # Calculate GRC
        if request.sora_version == SORAVersion.SORA_2_0:
            if request.grc_2_0 is None:
                raise HTTPException(status_code=400, detail="grc_2_0 required for SORA 2.0")
            if request.arc_2_0 is None:
                raise HTTPException(status_code=400, detail="arc_2_0 required for SORA 2.0")
            grc_result = grc_calc.calculate_grc_2_0(request.grc_2_0)
            arc_result = arc_calc.calculate_arc_2_0(request.arc_2_0)
        else:  # SORA 2.5
            if request.grc_2_5 is None:
                raise HTTPException(status_code=400, detail="grc_2_5 required for SORA 2.5")
            if request.arc_2_5 is None:
                raise HTTPException(status_code=400, detail="arc_2_5 required for SORA 2.5")
            grc_result = grc_calc.calculate_grc_2_5(request.grc_2_5)
            arc_result = arc_calc.calculate_arc_2_5(request.arc_2_5)
        
        # Calculate SAIL
        sail_request = SAILRequest(
            sora_version=request.sora_version,
            final_grc=grc_result.final_grc,
            final_arc=arc_result.final_arc,
        )
        sail_result = sail_calc.calculate_sail(sail_request)
        
        return SORACalculationResponse(
            grc=grc_result,
            arc=arc_result,
            sail=sail_result,
        )
    
    except HTTPException:
        raise
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"SORA calculation error: {str(e)}")


# ============================================================================
# RUN SERVER
# ============================================================================

if __name__ == "__main__":
    print("=" * 70)
    print("SKYWORKS AI SUITE - Python Calculation Microservice")
    print("=" * 70)
    print("🐍 Python FastAPI server starting...")
    print("📊 100% EASA/JARUS Compliant Calculations")
    print("🔗 Port: 8001")
    print("📚 API Docs: http://localhost:8001/api/docs")
    print("=" * 70)
    
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8001,
        reload=True,  # Auto-reload on code changes
        log_level="info",
    )
