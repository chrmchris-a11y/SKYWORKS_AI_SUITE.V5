from fastapi import APIRouter, HTTPException, status
from typing import Union
import logging

from .models.grc_models import GRCInputs20, GRCInputs25, GRCResult
from .calculators.grc_calculator import GRCCalculator20, GRCCalculator25
from .validators.grc_validator import GRCValidator

# Setup logging
logger = logging.getLogger(__name__)

# Initialize calculators and validator
calc_20 = GRCCalculator20()
calc_25 = GRCCalculator25()
validator = GRCValidator()

router = APIRouter(prefix="/sora", tags=["GRC - Ground Risk Class"])

@router.post("/2.0/grc", response_model=GRCResult, status_code=200)
def calculate_grc_20(request: GRCInputs20):
    """
    Calculate GRC for SORA 2.0 (EASA AMC/GM)
    
    **Process:**
    1. Determines iGRC from population density mapping
    2. Applies M1 strategic mitigation with floor cap algorithm
    3. Applies M2 impact effects mitigation
    4. Applies M3 emergency response plan adjustment
    5. Applies final floor at GRC=1
    
    **Key Features:**
    - M1 floor cap prevents over-reduction based on containment quality
    - Integer-only mitigation values (no fractional reductions)
    - Full calculation trace with official document references
    - Sequential mitigation application: iGRC → M1(+floor) → M2 → M3 → max(1, result)
    
    **Returns:** Complete GRC result with trace
    """
    try:
        # Validate inputs
        validation_errors = validator.validate_sora_20_inputs(request.model_dump())
        if validation_errors:
            logger.warning(f"SORA 2.0 validation errors: {validation_errors}")
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail={
                    "message": "Input validation failed",
                    "errors": validation_errors
                }
            )
        
        # Calculate GRC
        logger.info(f"Calculating SORA 2.0 GRC for population density: {request.population_density_p_km2}")
        result = calc_20.calculate(request)
        
        # Validate result
        result_errors = validator.validate_grc_result(result.model_dump())
        if result_errors:
            logger.error(f"SORA 2.0 result validation errors: {result_errors}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail={
                    "message": "Result validation failed",
                    "errors": result_errors
                }
            )
        
        logger.info(f"SORA 2.0 GRC calculation successful: {result.initial_grc} → {result.residual_grc}")
        return result
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"SORA 2.0 GRC calculation failed: {str(e)}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Internal calculation error: {str(e)}"
        )

@router.post("/2.5/grc", response_model=GRCResult, status_code=200)
def calculate_grc_25(request: GRCInputs25):
    """
    Calculate GRC for SORA 2.5 (JARUS)
    
    **Process:**
    1. Determines iGRC using quantitative model (dimension, speed, population)
    2. Applies M1A sheltering mitigation
    3. Applies M1B operational restrictions mitigation  
    4. Applies M1C ground observation mitigation
    5. Applies M1 floor cap if specified
    6. Applies M2 impact dynamics mitigation
    7. Applies final floor at GRC=1
    
    **Key Features:**
    - Quantitative iGRC model based on kinetic energy and exposure
    - N/A constraint validation (rejects invalid combinations)
    - Split M1 into M1A/M1B/M1C sub-mitigations
    - Integer-only mitigation values
    - Full calculation trace with official document references
    
    **N/A Constraints:**
    - M1A High = N/A (rejected with 422 error)
    - M1B Low = N/A (rejected with 422 error)  
    - M1C Medium/High = N/A (rejected with 422 error)
    - M2 Low = N/A (rejected with 422 error)
    
    **Returns:** Complete GRC result with trace
    """
    try:
        # Validate inputs (including N/A constraints)
        validation_errors = validator.validate_sora_25_inputs(request.model_dump())
        if validation_errors:
            logger.warning(f"SORA 2.5 validation errors: {validation_errors}")
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail={
                    "message": "Input validation failed (including N/A constraints)",
                    "errors": validation_errors
                }
            )
        
        # Calculate GRC
        logger.info(f"Calculating SORA 2.5 GRC for dimension: {request.characteristic_dimension_m}m, speed: {request.max_speed_mps}m/s")
        result = calc_25.calculate(request)
        
        # Validate result
        result_errors = validator.validate_grc_result(result.model_dump())
        if result_errors:
            logger.error(f"SORA 2.5 result validation errors: {result_errors}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail={
                    "message": "Result validation failed",
                    "errors": result_errors
                }
            )
        
        logger.info(f"SORA 2.5 GRC calculation successful: {result.initial_grc} → {result.residual_grc}")
        return result
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"SORA 2.5 GRC calculation failed: {str(e)}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Internal calculation error: {str(e)}"
        )

@router.get("/versions", status_code=200)
def get_supported_versions():
    """Get supported SORA versions and their capabilities"""
    return {
        "supported_versions": [
            {
                "version": "SORA_2.0",
                "standard": "EASA AMC/GM",
                "endpoint": "/sora/2.0/grc",
                "features": [
                    "Population density based iGRC",
                    "M1/M2/M3 mitigations",
                    "M1 floor cap algorithm",
                    "Integer-only reductions"
                ]
            },
            {
                "version": "SORA_2.5", 
                "standard": "JARUS",
                "endpoint": "/sora/2.5/grc",
                "features": [
                    "Quantitative iGRC model",
                    "M1A/M1B/M1C/M2 mitigations",
                    "N/A constraint validation",
                    "Integer-only reductions"
                ]
            }
        ]
    }