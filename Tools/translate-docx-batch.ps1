# Automated DOCX Translation Script with Claude API
# Translates all DOCX files in a folder to Greek using Claude 3.5 Haiku

param(
    [string]$SourceFolder = "C:\Users\chrmc\Desktop\FINAL DOCS IN GREEK FOR DCA CYPRUS_HELLONIC_DRONES",
    [string]$ClaudeApiKey = $env:ANTHROPIC_API_KEY
)

if (-not $ClaudeApiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable or pass -ClaudeApiKey."
}

Write-Host "🚀 Starting DOCX Translation to Greek..." -ForegroundColor Cyan
Write-Host "Source: $SourceFolder`n" -ForegroundColor Yellow

# Function to extract text from DOCX
function Get-DocxText {
    param([string]$DocxPath)
    
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        
        $tempDir = [System.IO.Path]::GetTempPath() + [Guid]::NewGuid().ToString()
        [System.IO.Compression.ZipFile]::ExtractToDirectory($DocxPath, $tempDir)
        
        $xmlPath = Join-Path $tempDir "word\document.xml"
        if (Test-Path $xmlPath) {
            [xml]$xml = Get-Content $xmlPath -Raw
            $text = ($xml.document.body.InnerText -replace '\s+', ' ').Trim()
            Remove-Item $tempDir -Recurse -Force
            return $text
        }
    }
    catch {
        Write-Host "❌ Error reading DOCX: $_" -ForegroundColor Red
        return $null
    }
}

# Function to translate text with Claude API
function Invoke-ClaudeTranslation {
    param([string]$Text)
    
    $headers = @{
        "x-api-key" = $ClaudeApiKey
        "anthropic-version" = "2023-06-01"
        "content-type" = "application/json"
    }
    
    $body = @{
        model = "claude-3-5-haiku-20241022"
        max_tokens = 8000
        messages = @(
            @{
                role = "user"
                content = "ΜΕΤΑΦΡΑΣΗ ΕΠΙΣΗΜΟΥ ΕΓΓΡΑΦΟΥ ΓΙΑ DCA CYPRUS

Μετάφρασε το παρακάτω έγγραφο στα Ελληνικά με:
- Σωστούς τεχνικούς όρους αεροπορίας/drones (EASA/JARUS)
- Διατήρηση επίσημου ύφους
- Ακρίβεια σε νομικούς όρους
- Διατήρηση δομής παραγράφων

ΚΕΙΜΕΝΟ:
$Text

Δώσε ΜΟΝΟ την ελληνική μετάφραση, χωρίς επεξηγήσεις."
            }
        )
    } | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" `
            -Method Post -Headers $headers -Body $body
        
        return $response.content[0].text
    }
    catch {
        Write-Host "❌ Claude API Error: $_" -ForegroundColor Red
        return $null
    }
}

# Function to create translated DOCX
function New-TranslatedDocx {
    param(
        [string]$OriginalPath,
        [string]$TranslatedText,
        [string]$OutputPath
    )
    
    try {
        # Copy original DOCX as base
        Copy-Item $OriginalPath $OutputPath -Force
        
        # Extract and modify
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $tempDir = [System.IO.Path]::GetTempPath() + [Guid]::NewGuid().ToString()
        [System.IO.Compression.ZipFile]::ExtractToDirectory($OutputPath, $tempDir)
        
        $xmlPath = Join-Path $tempDir "word\document.xml"
        [xml]$xml = Get-Content $xmlPath -Raw
        
        # Replace text (simplified - keeps structure)
        $paragraphs = $xml.document.body.p
        $sentences = $TranslatedText -split '\n\n'
        
        for ($i = 0; $i -lt [Math]::Min($paragraphs.Count, $sentences.Count); $i++) {
            if ($paragraphs[$i].r.t) {
                $paragraphs[$i].r.t = $sentences[$i]
            }
        }
        
        $xml.Save($xmlPath)
        
        # Repack
        Remove-Item $OutputPath -Force
        [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $OutputPath)
        Remove-Item $tempDir -Recurse -Force
        
        return $true
    }
    catch {
        Write-Host "❌ Error creating DOCX: $_" -ForegroundColor Red
        return $false
    }
}

# Main translation loop
$docxFiles = Get-ChildItem "$SourceFolder\*.docx" | Where-Object { $_.Name -notlike "*_GREEK*" }

Write-Host "📄 Found $($docxFiles.Count) files to translate`n" -ForegroundColor Green

foreach ($file in $docxFiles) {
    Write-Host "🔄 Processing: $($file.Name)" -ForegroundColor Cyan
    
    # Extract text
    Write-Host "   📖 Extracting text..." -ForegroundColor Gray
    $text = Get-DocxText $file.FullName
    
    if (-not $text) {
        Write-Host "   ❌ Failed to extract text, skipping...`n" -ForegroundColor Red
        continue
    }
    
    Write-Host "   📝 Text length: $($text.Length) characters" -ForegroundColor Gray
    
    # Translate
    Write-Host "   🌐 Translating with Claude..." -ForegroundColor Gray
    $translatedText = Invoke-ClaudeTranslation $text
    
    if (-not $translatedText) {
        Write-Host "   ❌ Translation failed, skipping...`n" -ForegroundColor Red
        continue
    }
    
    # Save
    $outputPath = Join-Path $SourceFolder ($file.BaseName + "_GREEK.docx")
    Write-Host "   💾 Saving: $($file.BaseName)_GREEK.docx" -ForegroundColor Gray
    
    $success = New-TranslatedDocx $file.FullName $translatedText $outputPath
    
    if ($success) {
        Write-Host "   ✅ DONE: $($file.BaseName)_GREEK.docx`n" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ Failed to save`n" -ForegroundColor Red
    }
    
    # Rate limit protection
    Start-Sleep -Seconds 2
}

Write-Host "`n🎉 Translation complete! Check folder for *_GREEK.docx files" -ForegroundColor Green
