"""
SORA 2.0 Python FastAPI Backend
Port: 8001
Purpose: GRC (Ground Risk Class) calculations for SORA 2.0

This service handles Ground Risk calculations according to JAR_doc_06 Table 3
Fixed M2 mitigation from "Medium" to "Low" per official JARUS SORA 2.0 spec
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field, validator
from enum import Enum
from typing import Optional
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Create FastAPI app
app = FastAPI(
    title="SORA 2.0 GRC Calculator",
    description="Ground Risk Class calculations per JAR_doc_06 Table 3",
    version="2.0.0"
)

# Enable CORS for frontend communication
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify exact origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ============================================================================
# MODELS - Fixed M2 Enum (None/Low/High only)
# ============================================================================

class M2Level20(str, Enum):
    """
    M2 - Impact Reduction Mitigation Levels for SORA 2.0
    
    Official Reference: JAR_doc_06 Table 3 - JARUS SORA 2.0 AMC
    
    CRITICAL FIX: Changed from None/Medium/High to None/Low/High
    """
    NONE = "None"
    LOW = "Low"      # ✅ FIXED: This is correct (was "Medium" before)
    HIGH = "High"
    
    @classmethod
    def get_value(cls, level: 'M2Level20') -> int:
        """Convert M2 enum to numeric value for calculations"""
        mapping = {
            cls.NONE: 0,
            cls.LOW: -1,   # ✅ FIXED: Low = -1 (per JAR_doc_06 Table 3)
            cls.HIGH: -2
        }
        return mapping[level]


class GrcRequestV20(BaseModel):
    """Request model for SORA 2.0 GRC calculation"""
    initial_grc: int = Field(..., ge=1, le=10, description="Initial GRC from iGRC table (1-10)")
    m1: int = Field(..., ge=-2, le=0, description="M1 - Strategic/Tactical mitigation (-2, -1, or 0)")
    m2: M2Level20 = Field(..., description="M2 - Impact reduction (None/Low/High only)")
    m3: int = Field(..., ge=-2, le=0, description="M3 - Effective deployment mitigation (-2, -1, or 0)")
    
    @validator('m2', pre=True)
    def validate_m2(cls, v):
        """
        Validate M2 value - REJECT "Medium" explicitly
        
        This validator ensures old "Medium" values are caught and rejected
        """
        if isinstance(v, str):
            # Normalize input
            v_normalized = v.strip().lower()
            
            # CRITICAL: Reject "Medium" explicitly
            if v_normalized == "medium":
                raise ValueError(
                    'Invalid M2 level "Medium". SORA 2.0 only supports None, Low, High. '
                    'Reference: JAR_doc_06 Table 3. '
                    'If you need Medium mitigation, use SORA 2.5 instead.'
                )
            
            # Map valid values
            valid_mapping = {
                "none": M2Level20.NONE,
                "low": M2Level20.LOW,
                "high": M2Level20.HIGH
            }
            
            if v_normalized not in valid_mapping:
                raise ValueError(
                    f'Invalid M2 level "{v}". Expected: None, Low, or High. '
                    'Reference: JAR_doc_06 Table 3.'
                )
            
            return valid_mapping[v_normalized]
        
        return v


class GrcResponseV20(BaseModel):
    """Response model for SORA 2.0 GRC calculation"""
    final_grc: int
    initial_grc: int
    m1: int
    m2: str
    m2_value: int
    m3: int
    calculation: str
    reference: str


# ============================================================================
# GRC CALCULATOR - Core Logic
# ============================================================================

def calculate_final_grc_sora_20(
    initial_grc: int,
    m1: int,
    m2: M2Level20,
    m3: int
) -> int:
    """
    Calculate Final GRC for SORA 2.0
    
    Formula: Final GRC = Initial GRC + M1 + M2 + M3
    
    Where:
    - Initial GRC: From iGRC table (Step #2) - Range: 1-10
    - M1: Strategic/Tactical mitigation - Values: 0, -1, -2
    - M2: Impact reduction mitigation - Values: 0 (None), -1 (Low), -2 (High)
    - M3: Effective deployment mitigation - Values: 0, -1, -2
    
    Official Reference: JAR_doc_06 Table 3 - JARUS SORA 2.0 AMC
    
    Returns:
        Final GRC (minimum value is 1)
    """
    logger.info(f"Calculating GRC - Initial: {initial_grc}, M1: {m1}, M2: {m2}, M3: {m3}")
    
    # Get M2 numeric value using the fixed mapping
    m2_value = M2Level20.get_value(m2)
    
    # Calculate Final GRC
    final_grc = initial_grc + m1 + m2_value + m3
    
    # Ensure Final GRC is at least 1 (per SORA 2.0 specification)
    final_grc = max(1, final_grc)
    
    logger.info(f"Final GRC: {final_grc} (calculation: {initial_grc} + {m1} + {m2_value} + {m3})")
    
    return final_grc


# ============================================================================
# API ENDPOINTS
# ============================================================================

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "service": "SORA 2.0 GRC Calculator",
        "port": 8001,
        "version": "2.0.0",
        "m2_levels": ["None (0)", "Low (-1)", "High (-2)"],
        "reference": "JAR_doc_06 Table 3"
    }


@app.post("/api/grc/calculate-v20", response_model=GrcResponseV20)
async def calculate_grc_v20(request: GrcRequestV20):
    """
    Calculate Final GRC for SORA 2.0
    
    This endpoint implements the corrected M2 mitigation levels:
    - None: 0
    - Low: -1   ✅ FIXED (was "Medium" before)
    - High: -2
    
    Official Reference: JAR_doc_06 Table 3 - JARUS SORA 2.0 AMC
    """
    try:
        logger.info(f"Received GRC calculation request: {request.dict()}")
        
        # Calculate Final GRC using corrected logic
        final_grc = calculate_final_grc_sora_20(
            initial_grc=request.initial_grc,
            m1=request.m1,
            m2=request.m2,
            m3=request.m3
        )
        
        # Get M2 numeric value for response
        m2_value = M2Level20.get_value(request.m2)
        
        # Build calculation string for transparency
        calculation = f"{request.initial_grc} + ({request.m1}) + ({m2_value}) + ({request.m3}) = {final_grc}"
        
        response = GrcResponseV20(
            final_grc=final_grc,
            initial_grc=request.initial_grc,
            m1=request.m1,
            m2=request.m2.value,
            m2_value=m2_value,
            m3=request.m3,
            calculation=calculation,
            reference="JAR_doc_06 Table 3 - SORA 2.0 AMC"
        )
        
        logger.info(f"GRC calculation successful: {response.dict()}")
        return response
        
    except ValueError as e:
        logger.error(f"Validation error: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"GRC calculation failed: {str(e)}")
        raise HTTPException(
            status_code=500,
            detail=f"Internal server error during GRC calculation: {str(e)}"
        )


@app.get("/")
async def root():
    """Root endpoint with service information"""
    return {
        "service": "SORA 2.0 GRC Calculator API",
        "version": "2.0.0",
        "docs": "/docs",
        "health": "/health",
        "endpoints": {
            "calculate_grc": "POST /api/grc/calculate-v20"
        },
        "important_fix": "M2 mitigation corrected from None/Medium/High to None/Low/High per JAR_doc_06 Table 3"
    }


# ============================================================================
# STARTUP MESSAGE
# ============================================================================

@app.on_event("startup")
async def startup_event():
    """Log startup message with important fix information"""
    logger.info("=" * 80)
    logger.info("SORA 2.0 GRC Calculator API Starting")
    logger.info("Port: 8001")
    logger.info("✅ FIXED: M2 mitigation levels corrected to None/Low/High")
    logger.info("Reference: JAR_doc_06 Table 3 - JARUS SORA 2.0 AMC")
    logger.info("=" * 80)


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001, log_level="info")
