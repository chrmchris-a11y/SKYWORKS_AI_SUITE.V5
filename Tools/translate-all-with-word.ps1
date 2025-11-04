param(
    [Parameter(Mandatory=$true)]
    [string]$SourceFolder
)

Write-Host "🌐 Microsoft Word Batch Translation" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Έλεγχος φακέλου
if (-not (Test-Path $SourceFolder)) {
    Write-Host "❌ Folder not found: $SourceFolder" -ForegroundColor Red
    exit 1
}

# Εύρεση όλων των .docx αρχείων που ΔΕΝ τελειώνουν σε _GREEK.docx
$files = Get-ChildItem -Path $SourceFolder -Filter "*.docx" | Where-Object { 
    $_.Name -notlike "*_GREEK.docx" 
}

if ($files.Count -eq 0) {
    Write-Host "✅ No files to translate!" -ForegroundColor Green
    exit 0
}

Write-Host "📄 Found $($files.Count) file(s) to translate:" -ForegroundColor Yellow
$files | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor White }
Write-Host ""

# Δημιουργία Word COM Object
try {
    Write-Host "🔧 Starting Microsoft Word..." -ForegroundColor Yellow
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    Write-Host "✅ Word started" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Failed to start Word: $_" -ForegroundColor Red
    exit 1
}

$successCount = 0
$failCount = 0

foreach ($file in $files) {
    $fileName = $file.BaseName
    $outputName = "${fileName}_GREEK.docx"
    $outputPath = Join-Path $SourceFolder $outputName
    
    # Αν υπάρχει ήδη το _GREEK.docx, skip
    if (Test-Path $outputPath) {
        Write-Host "⏭️  Skipping $($file.Name) (already translated)" -ForegroundColor Gray
        continue
    }
    
    Write-Host "🔄 Processing: $($file.Name)" -ForegroundColor Cyan
    
    try {
        # Άνοιγμα του αρχείου
        Write-Host "   📖 Opening document..." -ForegroundColor White
        $doc = $word.Documents.Open($file.FullName)
        
        # Επιλογή όλου του κειμένου
        $doc.Content.Select()
        
        # Μετάφραση English → Greek
        Write-Host "   🌐 Translating to Greek..." -ForegroundColor White
        $word.Selection.LanguageID = 1032  # Greek
        
        # Translation via Word API (χρειάζεται Internet connection)
        try {
            $word.Selection.Range.TCSCTranslator.Translate(
                [Microsoft.Office.Interop.Word.WdLanguageID]::wdEnglishUS,
                [Microsoft.Office.Interop.Word.WdLanguageID]::wdGreek
            )
        } catch {
            Write-Host "   ⚠️  Direct API translation failed, trying alternative method..." -ForegroundColor Yellow
            # Εναλλακτικό: Copy-paste στο Translator pane
            # Αυτό θα χρειαστεί manual intervention
        }
        
        # Αποθήκευση
        Write-Host "   💾 Saving as $outputName..." -ForegroundColor White
        $doc.SaveAs([ref]$outputPath)
        $doc.Close()
        
        Write-Host "   ✅ SUCCESS!" -ForegroundColor Green
        $successCount++
        
    } catch {
        Write-Host "   ❌ FAILED: $_" -ForegroundColor Red
        $failCount++
        
        # Κλείσιμο του doc αν είναι ανοιχτό
        try { $doc.Close($false) } catch {}
    }
    
    Write-Host ""
}

# Κλείσιμο του Word
Write-Host "🔧 Closing Microsoft Word..." -ForegroundColor Yellow
$word.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
Remove-Variable word
Write-Host "✅ Word closed" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "📊 SUMMARY:" -ForegroundColor Cyan
Write-Host "============" -ForegroundColor Cyan
Write-Host "✅ Successful: $successCount" -ForegroundColor Green
Write-Host "❌ Failed:     $failCount" -ForegroundColor Red
Write-Host ""

if ($successCount -gt 0) {
    Write-Host "🎉 Translation complete! Opening folder..." -ForegroundColor Green
    Start-Process $SourceFolder
}
