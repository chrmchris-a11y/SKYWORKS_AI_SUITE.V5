#!/bin/bash

# ============================================================================
# SORA 2.0 Services Test Script
# Tests both Python and .NET services to verify they're working correctly
# ============================================================================

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "========================================================================"
echo "SORA 2.0 Services Test Suite"
echo "========================================================================"
echo ""

TESTS_PASSED=0
TESTS_FAILED=0

# Helper function to run tests
run_test() {
    local test_name=$1
    local test_command=$2
    local expected_pattern=$3
    
    echo -n "Testing: $test_name... "
    
    result=$(eval "$test_command" 2>&1)
    
    if echo "$result" | grep -q "$expected_pattern"; then
        echo -e "${GREEN}✓ PASS${NC}"
        ((TESTS_PASSED++))
        return 0
    else
        echo -e "${RED}✗ FAIL${NC}"
        echo "  Expected pattern: $expected_pattern"
        echo "  Got: $result"
        ((TESTS_FAILED++))
        return 1
    fi
}

# ============================================================================
# TEST 1: Python Service Health
# ============================================================================

echo -e "${BLUE}=== Python Service Tests ===${NC}"
echo ""

run_test "Python health endpoint" \
    "curl -s http://localhost:8001/health" \
    "healthy"

run_test "Python service port" \
    "curl -s http://localhost:8001/health" \
    "8001"

# ============================================================================
# TEST 2: Python GRC Calculation with M2="Low"
# ============================================================================

echo ""
echo -e "${BLUE}=== Python GRC Calculation Tests ===${NC}"
echo ""

# Test 1: M2=Low should work
run_test "GRC calculation with M2=Low" \
    "curl -s -X POST http://localhost:8001/api/grc/calculate-v20 -H 'Content-Type: application/json' -d '{\"initial_grc\":5,\"m1\":-1,\"m2\":\"Low\",\"m3\":0}'" \
    "final_grc"

run_test "M2=Low returns -1 value" \
    "curl -s -X POST http://localhost:8001/api/grc/calculate-v20 -H 'Content-Type: application/json' -d '{\"initial_grc\":5,\"m1\":-1,\"m2\":\"Low\",\"m3\":0}'" \
    "\"m2_value\":-1"

run_test "Correct Final GRC calculation" \
    "curl -s -X POST http://localhost:8001/api/grc/calculate-v20 -H 'Content-Type: application/json' -d '{\"initial_grc\":5,\"m1\":-1,\"m2\":\"Low\",\"m3\":0}'" \
    "\"final_grc\":3"

# Test 2: M2=Medium should be rejected
echo -n "Testing: M2=Medium is rejected... "
result=$(curl -s -X POST http://localhost:8001/api/grc/calculate-v20 \
    -H 'Content-Type: application/json' \
    -d '{"initial_grc":5,"m1":-1,"m2":"Medium","m3":0}' 2>&1)

if echo "$result" | grep -q "Invalid M2 level"; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((TESTS_PASSED++))
else
    echo -e "${RED}✗ FAIL${NC}"
    echo "  Expected: Error rejecting Medium"
    echo "  Got: $result"
    ((TESTS_FAILED++))
fi

# Test 3: M2=None should work
run_test "GRC calculation with M2=None" \
    "curl -s -X POST http://localhost:8001/api/grc/calculate-v20 -H 'Content-Type: application/json' -d '{\"initial_grc\":5,\"m1\":-1,\"m2\":\"None\",\"m3\":0}'" \
    "\"m2_value\":0"

# Test 4: M2=High should work
run_test "GRC calculation with M2=High" \
    "curl -s -X POST http://localhost:8001/api/grc/calculate-v20 -H 'Content-Type: application/json' -d '{\"initial_grc\":5,\"m1\":-1,\"m2\":\"High\",\"m3\":0}'" \
    "\"m2_value\":-2"

# ============================================================================
# TEST 3: .NET Service Health
# ============================================================================

echo ""
echo -e "${BLUE}=== .NET Service Tests ===${NC}"
echo ""

run_test ".NET health endpoint" \
    "curl -s http://localhost:5210/api/sora/health" \
    "healthy"

run_test ".NET service port" \
    "curl -s http://localhost:5210/api/sora/health" \
    "5210"

run_test ".NET can see Python service" \
    "curl -s http://localhost:5210/api/sora/health" \
    "localhost:8001"

# ============================================================================
# TEST 4: Complete SORA Evaluation
# ============================================================================

echo ""
echo -e "${BLUE}=== Complete SORA Evaluation Tests ===${NC}"
echo ""

run_test "Complete SORA 2.0 evaluation" \
    "curl -s -X POST http://localhost:5210/api/sora/complete -H 'Content-Type: application/json' -d '{\"category\":\"SORA-2.0\",\"grcInputs\":{\"initialGrc\":5,\"m1\":-1,\"m2\":\"Low\",\"m3\":0},\"arcInputs\":{\"airspaceClass\":\"G\",\"altitude\":100}}'" \
    "success"

run_test "Final GRC in complete evaluation" \
    "curl -s -X POST http://localhost:5210/api/sora/complete -H 'Content-Type: application/json' -d '{\"category\":\"SORA-2.0\",\"grcInputs\":{\"initialGrc\":5,\"m1\":-1,\"m2\":\"Low\",\"m3\":0},\"arcInputs\":{\"airspaceClass\":\"G\",\"altitude\":100}}'" \
    "finalGrc"

run_test "SAIL determination" \
    "curl -s -X POST http://localhost:5210/api/sora/complete -H 'Content-Type: application/json' -d '{\"category\":\"SORA-2.0\",\"grcInputs\":{\"initialGrc\":5,\"m1\":-1,\"m2\":\"Low\",\"m3\":0},\"arcInputs\":{\"airspaceClass\":\"G\",\"altitude\":100}}'" \
    "sail"

run_test "M2 value in response" \
    "curl -s -X POST http://localhost:5210/api/sora/complete -H 'Content-Type: application/json' -d '{\"category\":\"SORA-2.0\",\"grcInputs\":{\"initialGrc\":5,\"m1\":-1,\"m2\":\"Low\",\"m3\":0},\"arcInputs\":{\"airspaceClass\":\"G\",\"altitude\":100}}'" \
    "\"m2\":\"Low\""

# ============================================================================
# TEST 5: Error Handling
# ============================================================================

echo ""
echo -e "${BLUE}=== Error Handling Tests ===${NC}"
echo ""

echo -n "Testing: Invalid category rejected... "
result=$(curl -s -X POST http://localhost:5210/api/sora/complete \
    -H 'Content-Type: application/json' \
    -d '{"category":"INVALID","grcInputs":{"initialGrc":5,"m1":-1,"m2":"Low","m3":0},"arcInputs":{}}' 2>&1)

if echo "$result" | grep -q "Unsupported category"; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((TESTS_PASSED++))
else
    echo -e "${RED}✗ FAIL${NC}"
    ((TESTS_FAILED++))
fi

echo -n "Testing: .NET rejects M2=Medium... "
result=$(curl -s -X POST http://localhost:5210/api/sora/complete \
    -H 'Content-Type: application/json' \
    -d '{"category":"SORA-2.0","grcInputs":{"initialGrc":5,"m1":-1,"m2":"Medium","m3":0},"arcInputs":{}}' 2>&1)

if echo "$result" | grep -q "Invalid M2"; then
    echo -e "${GREEN}✓ PASS${NC}"
    ((TESTS_PASSED++))
else
    echo -e "${RED}✗ FAIL${NC}"
    ((TESTS_FAILED++))
fi

# ============================================================================
# TEST SUMMARY
# ============================================================================

echo ""
echo "========================================================================"
echo "TEST SUMMARY"
echo "========================================================================"
echo ""

TOTAL_TESTS=$((TESTS_PASSED + TESTS_FAILED))

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ ALL TESTS PASSED!${NC}"
    echo ""
    echo "Passed: $TESTS_PASSED / $TOTAL_TESTS"
    echo ""
    echo "Your SORA 2.0 backend is working correctly! ✅"
    echo ""
    echo "Ready to use with frontend:"
    echo "  • Python API: http://localhost:8001"
    echo "  • .NET API:   http://localhost:5210"
    exit 0
else
    echo -e "${RED}✗ SOME TESTS FAILED${NC}"
    echo ""
    echo "Passed: $TESTS_PASSED / $TOTAL_TESTS"
    echo "Failed: $TESTS_FAILED / $TOTAL_TESTS"
    echo ""
    echo "Please check:"
    echo "  1. Both services are running"
    echo "  2. No firewall blocking ports 8001 or 5210"
    echo "  3. Check service logs for errors"
    echo ""
    exit 1
fi
