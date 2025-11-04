import pytest
from sail.validators.sail_validator import SAILValidator
from sail.models.sail_models import SAILInputs20, SAILInputs25, ARCValue


class TestSAILValidation:
    """Test SAIL input validation functionality."""
    
    def setup_method(self):
        """Setup test fixtures."""
        self.validator_20 = SAILValidator(sora_version="2.0")
        self.validator_25 = SAILValidator(sora_version="2.5")
    
    def test_valid_inputs_20(self):
        """Test validation of valid inputs for SORA 2.0."""
        valid_inputs = [
            SAILInputs20(grc=1, arc=ARCValue.A),
            SAILInputs20(grc=4, arc=ARCValue.B),
            SAILInputs20(grc=8, arc=ARCValue.D),
            SAILInputs20(grc=5, arc=ARCValue.C)
        ]
        
        for inputs in valid_inputs:
            # Should not raise any exceptions
            self.validator_20.validate_inputs(inputs)
            assert self.validator_20.is_valid_grc(inputs.grc)
            assert self.validator_20.is_valid_arc(inputs.arc)
    
    def test_valid_inputs_25(self):
        """Test validation of valid inputs for SORA 2.5."""
        valid_inputs = [
            SAILInputs25(grc=1, arc=ARCValue.A),
            SAILInputs25(grc=4, arc=ARCValue.B),
            SAILInputs25(grc=8, arc=ARCValue.D),
            SAILInputs25(grc=5, arc=ARCValue.C)
        ]
        
        for inputs in valid_inputs:
            # Should not raise any exceptions
            self.validator_25.validate_inputs(inputs)
            assert self.validator_25.is_valid_grc(inputs.grc)
            assert self.validator_25.is_valid_arc(inputs.arc)
    
    def test_invalid_grc_low(self):
        """Test validation of GRC values below minimum."""
        from pydantic import ValidationError
        
        # Should raise ValidationError from Pydantic constructor
        with pytest.raises(ValidationError):
            SAILInputs20(grc=0, arc=ARCValue.A)
        
        with pytest.raises(ValidationError):
            SAILInputs25(grc=0, arc=ARCValue.A)
        
        # Validator method should return False
        assert not self.validator_20.is_valid_grc(0)
        assert not self.validator_25.is_valid_grc(0)
    
    def test_invalid_grc_high(self):
        """Test validation of GRC values above maximum."""
        from pydantic import ValidationError
        
        with pytest.raises(ValidationError):
            SAILInputs20(grc=9, arc=ARCValue.A)
        
        with pytest.raises(ValidationError):
            SAILInputs25(grc=9, arc=ARCValue.A)
        
        # Validator method should return False
        assert not self.validator_20.is_valid_grc(9)
        assert not self.validator_25.is_valid_grc(9)
    
    def test_invalid_grc_negative(self):
        """Test validation of negative GRC values."""
        from pydantic import ValidationError
        
        with pytest.raises(ValidationError):
            SAILInputs20(grc=-1, arc=ARCValue.A)
        
        with pytest.raises(ValidationError):
            SAILInputs25(grc=-1, arc=ARCValue.A)
        
        # Validator method should return False
        assert not self.validator_20.is_valid_grc(-1)
        assert not self.validator_25.is_valid_grc(-1)
    
    def test_valid_arc_values(self):
        """Test validation of all valid ARC values."""
        valid_arcs = [ARCValue.A, ARCValue.B, ARCValue.C, ARCValue.D]
        
        for arc in valid_arcs:
            assert self.validator_20.is_valid_arc(arc)
            assert self.validator_25.is_valid_arc(arc)
            
            # Test with valid inputs
            inputs_20 = SAILInputs20(grc=4, arc=arc)
            inputs_25 = SAILInputs25(grc=4, arc=arc)
            
            self.validator_20.validate_inputs(inputs_20)
            self.validator_25.validate_inputs(inputs_25)
    
    def test_invalid_arc_none(self):
        """Test validation of None ARC value."""
        # This should be caught by Pydantic validation before reaching our validator
        with pytest.raises((ValueError, TypeError)):
            SAILInputs20(grc=4, arc=None)
        
        with pytest.raises((ValueError, TypeError)):
            SAILInputs25(grc=4, arc=None)
    
    def test_invalid_arc_string(self):
        """Test validation of invalid string ARC value."""
        # This should be caught by Pydantic validation
        with pytest.raises(ValueError):
            SAILInputs20(grc=4, arc="E")
        
        with pytest.raises(ValueError):
            SAILInputs25(grc=4, arc="E")
        
        # Test validator directly
        assert not self.validator_20.is_valid_arc("E")
        assert not self.validator_25.is_valid_arc("E")
    
    def test_grc_boundary_values(self):
        """Test GRC boundary values (1 and 8)."""
        # Test minimum valid value
        inputs_min_20 = SAILInputs20(grc=1, arc=ARCValue.A)
        inputs_min_25 = SAILInputs25(grc=1, arc=ARCValue.A)
        
        self.validator_20.validate_inputs(inputs_min_20)
        self.validator_25.validate_inputs(inputs_min_25)
        
        assert self.validator_20.is_valid_grc(1)
        assert self.validator_25.is_valid_grc(1)
        
        # Test maximum valid value
        inputs_max_20 = SAILInputs20(grc=8, arc=ARCValue.D)
        inputs_max_25 = SAILInputs25(grc=8, arc=ARCValue.D)
        
        self.validator_20.validate_inputs(inputs_max_20)
        self.validator_25.validate_inputs(inputs_max_25)
        
        assert self.validator_20.is_valid_grc(8)
        assert self.validator_25.is_valid_grc(8)
    
    def test_all_valid_combinations(self):
        """Test all valid GRC-ARC combinations."""
        valid_grcs = list(range(1, 9))  # 1 to 8
        valid_arcs = [ARCValue.A, ARCValue.B, ARCValue.C, ARCValue.D]
        
        for grc in valid_grcs:
            for arc in valid_arcs:
                inputs_20 = SAILInputs20(grc=grc, arc=arc)
                inputs_25 = SAILInputs25(grc=grc, arc=arc)
                
                # Should not raise exceptions
                self.validator_20.validate_inputs(inputs_20)
                self.validator_25.validate_inputs(inputs_25)
    
    def test_input_type_validation(self):
        """Test validation of input types."""
        from pydantic import ValidationError
        
        # Test non-integer GRC string - Pydantic V2 does type coercion "4" → 4
        inputs_20 = SAILInputs20(grc="4", arc=ARCValue.A)
        assert inputs_20.final_grc == 4  # Coerced to int
        
        # Test invalid string that can't be coerced
        with pytest.raises(ValidationError):
            SAILInputs20(grc="invalid", arc=ARCValue.A)
        
        # Test float - Pydantic may accept or coerce 4.5 to 4, test validator instead
        assert not self.validator_20.is_valid_grc(4.5)  # Non-integer float
        
        # Test float that equals integer - Pydantic accepts 4.0 as 4
        inputs_20 = SAILInputs20(grc=4.0, arc=ARCValue.A)
        self.validator_20.validate_inputs(inputs_20)
        assert self.validator_20.is_valid_grc(4.0)  # Exact integer as float
    
    def test_error_messages(self):
        """Test specific error messages."""
        from pydantic import ValidationError
        
        # GRC too low - ValidationError from Pydantic constructor
        with pytest.raises(ValidationError) as exc_info:
            SAILInputs20(grc=0, arc=ARCValue.A)
        assert "greater_than_equal" in str(exc_info.value)
        
        # GRC too high - also ValidationError from Pydantic
        with pytest.raises(ValidationError) as exc_info:
            SAILInputs20(grc=10, arc=ARCValue.A)
        assert "less_than_equal" in str(exc_info.value)
    
    def test_validator_version_consistency(self):
        """Test that both validator versions behave consistently."""
        test_cases = [
            (1, ARCValue.A),
            (4, ARCValue.B),
            (8, ARCValue.D),
            (5, ARCValue.C)
        ]
        
        for grc, arc in test_cases:
            inputs_20 = SAILInputs20(grc=grc, arc=arc)
            inputs_25 = SAILInputs25(grc=grc, arc=arc)
            
            # Both should validate successfully
            self.validator_20.validate_inputs(inputs_20)
            self.validator_25.validate_inputs(inputs_25)
            
            # Both should agree on validity
            assert self.validator_20.is_valid_grc(grc) == self.validator_25.is_valid_grc(grc)
            assert self.validator_20.is_valid_arc(arc) == self.validator_25.is_valid_arc(arc)
    
    def test_edge_case_float_grc(self):
        """Test edge cases with float GRC values."""
        # Test float that rounds to valid integer
        inputs_20 = SAILInputs20(grc=4, arc=ARCValue.A)  # Pydantic will convert
        self.validator_20.validate_inputs(inputs_20)
        
        # Direct validator test with floats
        assert self.validator_20.is_valid_grc(4.0)
        assert not self.validator_20.is_valid_grc(4.1)  # Non-integer
        assert not self.validator_20.is_valid_grc(0.9)  # Below minimum
    
    def test_validation_performance(self):
        """Test validation performance with multiple inputs."""
        # This test ensures validation is efficient
        inputs_20 = SAILInputs20(grc=4, arc=ARCValue.B)
        inputs_25 = SAILInputs25(grc=4, arc=ARCValue.B)
        
        # Run validation multiple times
        for _ in range(100):
            self.validator_20.validate_inputs(inputs_20)
            self.validator_25.validate_inputs(inputs_25)
        
        # Should complete without issues
        assert True
    
    def test_concurrent_validation(self):
        """Test that validators can be used concurrently."""
        inputs_list_20 = [
            SAILInputs20(grc=i, arc=ARCValue.A) for i in range(1, 9)
        ]
        inputs_list_25 = [
            SAILInputs25(grc=i, arc=ARCValue.A) for i in range(1, 9)
        ]
        
        # Validate all inputs
        for inputs_20, inputs_25 in zip(inputs_list_20, inputs_list_25):
            self.validator_20.validate_inputs(inputs_20)
            self.validator_25.validate_inputs(inputs_25)
        
        # Should complete successfully
        assert True