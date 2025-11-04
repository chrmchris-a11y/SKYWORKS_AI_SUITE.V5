#!/usr/bin/env python3
"""
Generate HTML Comprehensive Accuracy Report for SORA Calculators
Reads output from test_comprehensive_report.py
"""
import subprocess
import sys
from datetime import datetime
from pathlib import Path

def generate_html_report():
    """Run tests and generate styled HTML report"""
    
    print("Running comprehensive tests...")
    
    # Run the comprehensive test and capture output
    result = subprocess.run(
        [sys.executable, "test_comprehensive_report.py"],
        capture_output=True,
        text=True,
        encoding='utf-8',
        errors='replace',
        cwd=Path(__file__).parent
    )
    
    output = result.stdout or ""
    exit_code = result.returncode
    
    # Strip ANSI codes and box-drawing characters
    import re
    ansi_escape = re.compile(r'\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])')
    output = ansi_escape.sub('', output)
    # Replace box drawing with plain =
    output = output.replace('═', '=').replace('━', '-')
    
    print(f"Test output captured: {len(output)} chars")
    
    # Parse output - looking for key summary lines
    passed_count = 20  # Default from visible output
    failed_count = 0
    error_count = 0
    total_count = 20
<html lang="el">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>SKYWORKS SORA - Comprehensive Accuracy Report</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            color: #333;
            padding: 20px;
            min-height: 100vh;
        }}
        
        .container {{
            max-width: 1400px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            overflow: hidden;
        }}
        
        header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }}
        
        header h1 {{
            font-size: 2.5rem;
            margin-bottom: 10px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
        }}
        
        header .subtitle {{
            font-size: 1.1rem;
            opacity: 0.95;
        }}
        
        header .timestamp {{
            margin-top: 15px;
            font-size: 0.9rem;
            opacity: 0.8;
        }}
        
        .summary {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            padding: 40px;
            background: #f8f9fa;
        }}
        
        .stat-card {{
            background: white;
            padding: 25px;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            text-align: center;
            transition: transform 0.2s;
        }}
        
        .stat-card:hover {{
            transform: translateY(-5px);
            box-shadow: 0 8px 12px rgba(0,0,0,0.15);
        }}
        
        .stat-value {{
            font-size: 3rem;
            font-weight: bold;
            margin: 10px 0;
        }}
        
        .stat-label {{
            color: #666;
            font-size: 0.9rem;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        
        .passed {{ color: #28a745; }}
        .failed {{ color: #dc3545; }}
        .errors {{ color: #ffc107; }}
        
        .tests-section {{
            padding: 40px;
        }}
        
        .test-group {{
            margin-bottom: 40px;
        }}
        
        .test-group h2 {{
            color: #667eea;
            border-bottom: 3px solid #667eea;
            padding-bottom: 10px;
            margin-bottom: 20px;
            font-size: 1.8rem;
        }}
        
        .test-item {{
            padding: 15px 20px;
            margin: 10px 0;
            border-radius: 6px;
            border-left: 4px solid;
            background: #f8f9fa;
            display: flex;
            align-items: center;
            gap: 15px;
        }}
        
        .test-item.pass {{
            border-left-color: #28a745;
            background: #d4edda;
        }}
        
        .test-item.fail {{
            border-left-color: #dc3545;
            background: #f8d7da;
        }}
        
        .test-item.error {{
            border-left-color: #ffc107;
            background: #fff3cd;
        }}
        
        .test-icon {{
            font-size: 1.5rem;
            min-width: 30px;
        }}
        
        .test-name {{
            flex: 1;
            font-weight: 500;
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
            margin-top: 20px;
        }}
        
        .badge.success {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }}
        
        .badge.warning {{
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
        }}
        
        .details {{
            margin-top: 10px;
            padding: 10px;
            background: rgba(0,0,0,0.05);
            border-radius: 4px;
            font-size: 0.9rem;
            font-family: 'Courier New', monospace;
        }}
    </style>
</head>
<body>
    <div class="container">
        <header>
            <h1>🚁 SKYWORKS AI SUITE</h1>
            <div class="subtitle">SORA Calculator - Comprehensive Accuracy Report</div>
            <div class="timestamp">Generated: {datetime.now().strftime('%d/%m/%Y %H:%M:%S')}</div>
        </header>
"""
    
    # Parse summary
    passed_count = 0
    failed_count = 0
    error_count = 0
    total_count = 0
    
    test_results = {
        'GRC SORA 2.0': [],
        'GRC SORA 2.5': [],
        'ARC SORA 2.0 & 2.5': [],
        'SAIL CALCULATOR': []
    }
    
    current_section = None
    
    for line in lines:
        if 'PASSED:' in line and '/' in line:
            parts = line.split('/')
            passed_count = int(parts[0].split()[-1])
            total_count = int(parts[1].strip())
        elif 'FAILED:' in line and '/' in line:
            failed_count = int(line.split('/')[0].split()[-1])
        elif 'ERRORS:' in line and '/' in line:
            error_count = int(line.split('/')[0].split()[-1])
        
        # Detect sections
        if 'GRC SORA 2.0 TESTS' in line:
            current_section = 'GRC SORA 2.0'
        elif 'GRC SORA 2.5 TESTS' in line:
            current_section = 'GRC SORA 2.5'
        elif 'ARC SORA 2.0 & 2.5 TESTS' in line:
            current_section = 'ARC SORA 2.0 & 2.5'
        elif 'SAIL CALCULATOR TESTS' in line:
            current_section = 'SAIL CALCULATOR'
        
        # Parse test results
        if current_section and (line.startswith('✅') or line.startswith('❌') or line.startswith('⚠️')):
            test_results[current_section].append(line.strip())
    
    # Summary cards
    html += f"""
        <div class="summary">
            <div class="stat-card">
                <div class="stat-label">Total Tests</div>
                <div class="stat-value">{total_count}</div>
            </div>
            <div class="stat-card">
                <div class="stat-label">Passed</div>
                <div class="stat-value passed">✅ {passed_count}</div>
            </div>
            <div class="stat-card">
                <div class="stat-label">Failed</div>
                <div class="stat-value failed">❌ {failed_count}</div>
            </div>
            <div class="stat-card">
                <div class="stat-label">Errors</div>
                <div class="stat-value errors">⚠️ {error_count}</div>
            </div>
        </div>
"""
    
    # Test details
    html += '<div class="tests-section">'
    
    for section, tests in test_results.items():
        if not tests:
            continue
            
        html += f'<div class="test-group"><h2>{section}</h2>'
        
        for test_line in tests:
            icon = test_line[0]
            name = test_line[2:].strip()
            
            if '✅' in test_line:
                css_class = 'pass'
            elif '❌' in test_line:
                css_class = 'fail'
            else:
                css_class = 'error'
            
            html += f'''
            <div class="test-item {css_class}">
                <div class="test-icon">{icon}</div>
                <div class="test-name">{name}</div>
            </div>
            '''
        
        html += '</div>'
    
    html += '</div>'
    
    # Footer with overall status
    if failed_count == 0 and error_count == 0:
        badge_class = 'success'
        badge_text = '🎉 ALL TESTS PASSED - JARUS COMPLIANT'
    else:
        badge_class = 'warning'
        badge_text = '⚠️ ATTENTION REQUIRED - Review Failed Tests'
    
    html += f"""
        <footer>
            <div class="badge {badge_class}">{badge_text}</div>
            <p style="margin-top: 20px; opacity: 0.8;">
                SORA 2.0: JARUS JAR-DEL-WG6-D.04 Edition 2.0<br>
                SORA 2.5: JARUS JAR-DEL-SRM-SORA-MB-2.5
            </p>
        </footer>
    </div>
</body>
</html>
"""
    
    # Write report
    report_path = Path(__file__).parent / "COMPREHENSIVE_ACCURACY_REPORT.html"
    report_path.write_text(html, encoding='utf-8')
    
    print(f"✅ HTML Report generated: {report_path}")
    print(f"📊 Results: {passed_count}/{total_count} passed")
    
    return exit_code

if __name__ == "__main__":
    sys.exit(generate_html_report())
