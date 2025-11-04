# Convert DOCX to TXT for translation
# Usage: .\convert-docx-to-txt.ps1

$sourceFolder = "C:\Users\chrmc\Desktop\FINAL DOCS IN GREEK FOR DCA CYPRUS_HELLENIC_DRONES"
$outputFolder = "$sourceFolder\txt_versions"

# Create output folder
if (-not (Test-Path $outputFolder)) {
    New-Item -ItemType Directory -Path $outputFolder | Out-Null
}

# Create Word COM object
$word = New-Object -ComObject Word.Application
$word.Visible = $false

Get-ChildItem "$sourceFolder\*.docx" | ForEach-Object {
    Write-Host "Converting: $($_.Name)"
    
    try {
        # Open DOCX
        $doc = $word.Documents.Open($_.FullName)
        
        # Save as TXT
        $txtPath = Join-Path $outputFolder ($_.BaseName + ".txt")
        $doc.SaveAs([ref]$txtPath, [ref]2) # 2 = wdFormatText
        
        $doc.Close()
        Write-Host "✅ Saved: $txtPath" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Error: $_" -ForegroundColor Red
    }
}

$word.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
Remove-Variable word

Write-Host "`n✅ Conversion complete! Files in: $outputFolder" -ForegroundColor Green
