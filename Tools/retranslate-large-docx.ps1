# Re-translate large DOCX files with Claude Sonnet 3.5 (larger output limit)
# Usage: .\retranslate-large-docx.ps1 -FilePath "path\to\file.docx"

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,
    [string]$ClaudeApiKey = $env:ANTHROPIC_API_KEY
)

if (-not $ClaudeApiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable or pass -ClaudeApiKey."
}

Write-Host "🚀 Re-translating large file with Claude Sonnet 3.5..." -ForegroundColor Cyan

# Extract text from DOCX
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
        Write-Host "❌ Error: $_" -ForegroundColor Red
        return $null
    }
}

# Translate with Claude Sonnet 3.5 (large output limit)
function Invoke-ClaudeSonnetTranslation {
    param([string]$Text)
    
    $headers = @{
        "x-api-key" = $ClaudeApiKey
        "anthropic-version" = "2023-06-01"
        "content-type" = "application/json"
    }
    
    Write-Host "📝 Text length: $($Text.Length) characters" -ForegroundColor Gray
    Write-Host "🌐 Translating with Claude Sonnet 3.5 (max 200K tokens output)..." -ForegroundColor Yellow
    
    $body = @{
        model = "claude-opus-4-20250514"
        max_tokens = 100000
        messages = @(
            @{
                role = "user"
                content = "ΜΕΤΑΦΡΑΣΗ ΕΠΙΣΗΜΟΥ ΕΓΓΡΑΦΟΥ ΓΙΑ DCA CYPRUS

Μετάφρασε ΟΛΟ το παρακάτω έγγραφο στα Ελληνικά με:
- Σωστούς τεχνικούς όρους αεροπορίας/drones (EASA/JARUS/PDRA)
- Διατήρηση επίσημου ύφους
- Ακρίβεια σε νομικούς όρους
- Διατήρηση δομής παραγράφων

ΣΗΜΑΝΤΙΚΟ: Μετάφρασε ΟΛΕς τις σελίδες, μην παραλείψεις τίποτα!

ΚΕΙΜΕΝΟ:
$Text

Δώσε ΜΟΝΟ την πλήρη ελληνική μετάφραση, χωρίς επεξηγήσεις."
            }
        )
    } | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" `
            -Method Post -Headers $headers -Body $body -TimeoutSec 300
        
        Write-Host "✅ Translation complete! Output: $($response.content[0].text.Length) characters" -ForegroundColor Green
        return $response.content[0].text
    }
    catch {
        Write-Host "❌ API Error: $_" -ForegroundColor Red
        return $null
    }
}

# Create translated DOCX
function New-TranslatedDocx {
    param(
        [string]$OriginalPath,
        [string]$TranslatedText,
        [string]$OutputPath
    )
    
    try {
        Copy-Item $OriginalPath $OutputPath -Force
        
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $tempDir = [System.IO.Path]::GetTempPath() + [Guid]::NewGuid().ToString()
        [System.IO.Compression.ZipFile]::ExtractToDirectory($OutputPath, $tempDir)
        
        $xmlPath = Join-Path $tempDir "word\document.xml"
        [xml]$xml = Get-Content $xmlPath -Raw
        
        $paragraphs = $xml.document.body.p
        $sentences = $TranslatedText -split '\n\n'
        
        for ($i = 0; $i -lt [Math]::Min($paragraphs.Count, $sentences.Count); $i++) {
            if ($paragraphs[$i].r.t) {
                $paragraphs[$i].r.t = $sentences[$i]
            }
        }
        
        $xml.Save($xmlPath)
        
        Remove-Item $OutputPath -Force
        [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $OutputPath)
        Remove-Item $tempDir -Recurse -Force
        
        return $true
    }
    catch {
        Write-Host "❌ Error: $_" -ForegroundColor Red
        return $false
    }
}

# Main
Write-Host "📄 File: $FilePath`n" -ForegroundColor Cyan

$text = Get-DocxText $FilePath
if (-not $text) {
    Write-Host "❌ Failed to extract text" -ForegroundColor Red
    exit 1
}

$translatedText = Invoke-ClaudeSonnetTranslation $text
if (-not $translatedText) {
    Write-Host "❌ Translation failed" -ForegroundColor Red
    exit 1
}

$outputPath = $FilePath -replace "\.docx$", "_GREEK.docx" -replace "_GREEK_GREEK", "_GREEK"
Write-Host "`n💾 Saving: $outputPath" -ForegroundColor Cyan

$success = New-TranslatedDocx $FilePath $translatedText $outputPath

if ($success) {
    Write-Host "`n🎉 SUCCESS! Translated file: $outputPath" -ForegroundColor Green
    Write-Host "Cost: ~$1-2 for this file (Sonnet is more expensive but handles large files)" -ForegroundColor Yellow
}
else {
    Write-Host "`n❌ Failed to create translated DOCX" -ForegroundColor Red
    exit 1
}
