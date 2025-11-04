#!/usr/bin/env python3
"""
Simple HTML Report Generator for SORA Calculator Tests
"""
import subprocess
import sys
from datetime import datetime
from pathlib import Path

def run_tests_and_generate_html():
    """Run comprehensive tests and create HTML report"""
    
    print("🧪 Running comprehensive SORA calculator tests...")
    
    # Run tests
    result = subprocess.run(
        [sys.executable, "test_comprehensive_report.py"],
        capture_output=True,
        text=True,
        encoding='utf-8',
        errors='replace',
        cwd=Path(__file__).parent
    )
    
    output = result.stdout or ""
    
    # Count results
    passed = output.count('✅')
    failed = output.count('❌') + output.count('⚠️')
    total = passed + failed
    
    # Extract test lines by section
    lines = output.split('\n')
    sections = {
        'GRC SORA 2.0': [],
        'GRC SORA 2.5': [],
        'ARC SORA 2.0 & 2.5': [],
        'SAIL CALCULATOR': []
    }
    
    current = None
    for line in lines:
        if 'GRC SORA 2.0 TESTS' in line:
            current = 'GRC SORA 2.0'
        elif 'GRC SORA 2.5 TESTS' in line:
            current = 'GRC SORA 2.5'
        elif 'ARC SORA 2.0 & 2.5 TESTS' in line:
            current = 'ARC SORA 2.0 & 2.5'
        elif 'SAIL CALCULATOR TESTS' in line:
            current = 'SAIL CALCULATOR'
        elif current and ('✅' in line or '❌' in line or '⚠️' in line):
            sections[current].append(line.strip())
    
    # Generate HTML
    status = "🎉 ALL TESTS PASSED" if failed == 0 else "⚠️ SOME TESTS FAILED"
    status_class = "success" if failed == 0 else "warning"
    
    html = f"""<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>SKYWORKS SORA - Test Report</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', Tahoma, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            min-height: 100vh;
        }}
        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        header {{
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }}
        h1 {{ font-size: 2.5rem; margin-bottom: 10px; }}
        .timestamp {{ opacity: 0.8; margin-top: 15px; }}
        .stats {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            padding: 40px;
            background: #f8f9fa;
        }}
        .stat {{
            background: white;
            padding: 25px;
            border-radius: 8px;
            text-align: center;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }}
        .stat-value {{
            font-size: 3rem;
            font-weight: bold;
            margin: 10px 0;
        }}
        .stat-label {{
            color: #666;
            text-transform: uppercase;
            font-size: 0.9rem;
        }}
        .passed {{ color: #28a745; }}
        .failed {{ color: #dc3545; }}
        .tests {{ padding: 40px; }}
        .section {{ margin-bottom: 40px; }}
        .section h2 {{
            color: #667eea;
            border-bottom: 3px solid #667eea;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }}
        .test-line {{
            padding: 15px 20px;
            margin: 10px 0;
            border-radius: 6px;
            border-left: 4px solid #28a745;
            background: #d4edda;
        }}
        .test-line.fail {{
            border-left-color: #dc3545;
            background: #f8d7da;
        }}
        footer {{
            background: #2c3e50;
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .badge {{
            display: inline-block;
            padding: 10px 30px;
            border-radius: 50px;
            font-size: 1.2rem;
            font-weight: bold;
            margin: 20px 0;
        }}
        .badge.success {{ background: #28a745; color: white; }}
        .badge.warning {{ background: #dc3545; color: white; }}
    </style>
</head>
<body>
    <div class="container">
        <header>
            <h1>🚁 SKYWORKS AI SUITE</h1>
            <div>SORA Calculator - Comprehensive Test Report</div>
            <div class="timestamp">{datetime.now().strftime('%d/%m/%Y %H:%M:%S')}</div>
        </header>
        
        <div class="stats">
            <div class="stat">
                <div class="stat-label">Total Tests</div>
                <div class="stat-value">{total}</div>
            </div>
            <div class="stat">
                <div class="stat-label">Passed</div>
                <div class="stat-value passed">{passed}</div>
            </div>
            <div class="stat">
                <div class="stat-label">Failed</div>
                <div class="stat-value failed">{failed}</div>
            </div>
        </div>
        
        <div class="tests">
"""
    
    for section, tests in sections.items():
        if tests:
            html += f'<div class="section"><h2>{section}</h2>\n'
            for test in tests:
                css_class = '' if '✅' in test else 'fail'
                html += f'<div class="test-line {css_class}">{test}</div>\n'
            html += '</div>\n'
    
    html += f"""
        </div>
        
        <footer>
            <div class="badge {status_class}">{status}</div>
            <p style="margin-top: 20px; opacity: 0.8;">
                SORA 2.0: JARUS JAR-DEL-WG6-D.04<br>
                SORA 2.5: JARUS JAR-DEL-SRM-SORA-MB-2.5
            </p>
        </footer>
    </div>
</body>
</html>
"""
    
    # Save report
    report_file = Path(__file__).parent / "COMPREHENSIVE_ACCURACY_REPORT.html"
    report_file.write_text(html, encoding='utf-8')
    
    print(f"✅ Report generated: {report_file}")
    print(f"📊 Results: {passed} passed, {failed} failed, {total} total")
    
    return 0 if failed == 0 else 1

if __name__ == "__main__":
    sys.exit(run_tests_and_generate_html())
