from fastapi import APIRouter, HTTPException, status
from pathlib import Path
import logging

from arc.models.arc_models import ARCInputs20, ARCInputs25, ARCResult
from arc.calculators.arc_calculator import ARCCalculator20, ARCCalculator25
from arc.validators.arc_validator import ARCValidator

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/sora", tags=["ARC - Air Risk Class"])

# Initialize calculators
calc_20 = ARCCalculator20()
calc_25 = ARCCalculator25()

@router.post("/2.0/arc", response_model=ARCResult, status_code=200)
async def calculate_arc_sora_20(request: ARCInputs20):
    """
    Calculate ARC (Air Risk Class) per EASA SORA 2.0
    
    Determines Initial ARC from operational parameters and applies strategic 
    mitigations with strict reduction caps per EASA AMC/GM Annex C.
    
    Key Rules:
    - Initial ARC (a-d) determined from Table C.1
    - Strategic mitigations applied per Table C.2  
    - Default cap: ≤1 class reduction
    - With certified segregation: ≤2 classes
    - Geo-fencing = supporting evidence only (0 direct reduction)
    - Returns residual ARC + full calculation trace
    """
    try:
        # Additional validation
        validation_errors = ARCValidator.validate_sora_20_inputs(request.model_dump())
        if validation_errors:
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail={"errors": validation_errors}
            )
        
        # Calculate ARC
        result = calc_20.calculate(request)
        
        # Validate result
        result_errors = ARCValidator.validate_arc_result(result.model_dump())
        if result_errors:
            logger.error(f"Invalid ARC calculation result: {result_errors}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal calculation error"
            )
        
        logger.info(f"SORA 2.0 ARC calculated: {result.initial_arc} -> {result.residual_arc}")
        return result
        
    except ValidationError as e:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"ARC 2.0 calculation failed: {e}", extra={"request": request.model_dump()})
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal calculation error"
        )

@router.post("/2.5/arc", response_model=ARCResult, status_code=200)
async def calculate_arc_sora_25(request: ARCInputs25):
    """
    Calculate ARC per JARUS SORA 2.5 Annex C
    
    Enhanced strategic mitigation mechanisms with:
    - U-space services support (as service arrangement evidence)
    - Data-driven traffic density assessment
    - Improved containment mechanisms
    - Full traceability to JARUS Annex C
    
    Key Rules:
    - U-space services = supporting evidence within cap (not automatic -1)
    - Certified containment ≤2 classes
    - DAA excluded from strategic ARC (tactical layer)
    - Full integer reductions only
    """
    try:
        # Additional validation
        validation_errors = ARCValidator.validate_sora_25_inputs(request.model_dump())
        if validation_errors:
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail={"errors": validation_errors}
            )
        
        # Calculate ARC
        result = calc_25.calculate(request)
        
        # Validate result
        result_errors = ARCValidator.validate_arc_result(result.model_dump())
        if result_errors:
            logger.error(f"Invalid ARC calculation result: {result_errors}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal calculation error"
            )
        
        logger.info(f"SORA 2.5 ARC calculated: {result.initial_arc} -> {result.residual_arc}")
        return result
        
    except ValidationError as e:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"ARC 2.5 calculation failed: {e}", extra={"request": request.model_dump()})
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal calculation error"
        )

@router.get("/arc/versions", status_code=200)
async def get_arc_versions():
    """Get available ARC calculation versions and rule information"""
    return {
        "versions": {
            "SORA_2.0": {
                "source": "EASA_EAR_UAS_2024",
                "publication_date": "2024-07-01",
                "annex_c_version": "AMC1_Article_11_2024-07",
                "rules_hash": "sha256:abc123def456",
                "endpoint": "/sora/2.0/arc"
            },
            "SORA_2.5": {
                "source": "JARUS_SORA_v2.5_Annex_C_v1.0", 
                "publication_date": "2024-01-15",
                "annex_c_version": "v1.0",
                "rules_hash": "sha256:def456ghi789",
                "endpoint": "/sora/2.5/arc"
            }
        },
        "supported_features": {
            "initial_arc_determination": True,
            "strategic_mitigations": True,
            "calculation_trace": True,
            "validation": True,
            "integer_reductions_only": True,
            "daa_excluded_from_strategic": True
        }
    }
