# Translate large DOCX in chunks with Claude Opus 4
# Splits text into chunks, translates each, then merges

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,
    [string]$ClaudeApiKey = $env:ANTHROPIC_API_KEY,
    [int]$ChunkSize = 20000  # Characters per chunk (~5K tokens)
)

if (-not $ClaudeApiKey) {
    throw "Anthropic API key not provided. Set ANTHROPIC_API_KEY environment variable or pass -ClaudeApiKey."
}

Write-Host "🚀 Translating large file in chunks with Claude Opus 4..." -ForegroundColor Cyan

# Extract text
function Get-DocxText {
    param([string]$DocxPath)
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $tempDir = [System.IO.Path]::GetTempPath() + [Guid]::NewGuid().ToString()
        [System.IO.Compression.ZipFile]::ExtractToDirectory($DocxPath, $tempDir)
        $xmlPath = Join-Path $tempDir "word\document.xml"
        if (Test-Path $xmlPath) {
            [xml]$xml = Get-Content $xmlPath -Raw
            $text = $xml.document.body.InnerText.Trim()
            Remove-Item $tempDir -Recurse -Force
            return $text
        }
    }
    catch {
        Write-Host "❌ Error: $_" -ForegroundColor Red
        return $null
    }
}

# Translate chunk with Opus 4
function Invoke-ClaudeChunkTranslation {
    param([string]$Text, [int]$ChunkNumber, [int]$TotalChunks)
    
    Write-Host "   🌐 Translating chunk $ChunkNumber/$TotalChunks ($($Text.Length) chars)..." -ForegroundColor Yellow
    
    $headers = @{
        "x-api-key" = $ClaudeApiKey
        "anthropic-version" = "2023-06-01"
        "content-type" = "application/json"
    }
    
    $body = @{
        model = "claude-opus-4-20250514"
        max_tokens = 28000
        messages = @(
            @{
                role = "user"
                content = "ΜΕΤΑΦΡΑΣΗ ΕΠΙΣΗΜΟΥ ΕΓΓΡΑΦΟΥ (CHUNK $ChunkNumber/$TotalChunks)

Μετάφρασε ΟΛΟΚΛΗΡΟ το παρακάτω τμήμα στα Ελληνικά με:
- Σωστούς τεχνικούς όρους EASA/JARUS/PDRA
- Επίσημο ύφος
- Ακριβή νομική ορολογία
- Διατήρηση παραγράφων

ΣΗΜΑΝΤΙΚΟ: Μην παραλείψεις ΤΙΠΟΤΑ! Δώσε ΜΟΝΟ την ελληνική μετάφραση.

ΚΕΙΜΕΝΟ:
$Text"
            }
        )
    } | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri "https://api.anthropic.com/v1/messages" `
            -Method Post -Headers $headers -Body $body -TimeoutSec 300
        
        return $response.content[0].text
    }
    catch {
        Write-Host "   ❌ API Error: $_" -ForegroundColor Red
        return $null
    }
}

# Main
Write-Host "📄 File: $(Split-Path $FilePath -Leaf)`n" -ForegroundColor Cyan

# Extract
Write-Host "📖 Extracting text..." -ForegroundColor Gray
$text = Get-DocxText $FilePath
if (-not $text) {
    Write-Host "❌ Failed to extract text" -ForegroundColor Red
    exit 1
}

Write-Host "   Text length: $($text.Length) characters`n" -ForegroundColor Gray

# Split into chunks
$chunks = @()
$start = 0
while ($start -lt $text.Length) {
    $length = [Math]::Min($ChunkSize, $text.Length - $start)
    $chunk = $text.Substring($start, $length)
    $chunks += $chunk
    $start += $length
}

Write-Host "✂️ Split into $($chunks.Count) chunks`n" -ForegroundColor Green

# Translate each chunk
$translatedChunks = @()
for ($i = 0; $i -lt $chunks.Count; $i++) {
    Write-Host "🔄 Processing chunk $($i+1)/$($chunks.Count)..." -ForegroundColor Cyan
    
    $translated = Invoke-ClaudeChunkTranslation $chunks[$i] ($i+1) $chunks.Count
    
    if (-not $translated) {
        Write-Host "❌ Chunk $($i+1) failed!" -ForegroundColor Red
        exit 1
    }
    
    $translatedChunks += $translated
    Write-Host "   ✅ Chunk $($i+1) done!`n" -ForegroundColor Green
    
    # Rate limit protection
    if ($i -lt $chunks.Count - 1) {
        Start-Sleep -Seconds 3
    }
}

# Merge
Write-Host "🔗 Merging chunks..." -ForegroundColor Cyan
$fullTranslation = $translatedChunks -join "`n`n"
Write-Host "   Total translated: $($fullTranslation.Length) characters`n" -ForegroundColor Gray

# Save to new DOCX
$outputPath = $FilePath -replace "\.docx$", "_GREEK.docx" -replace "_GREEK_GREEK", "_GREEK"

Write-Host "💾 Saving to: $(Split-Path $outputPath -Leaf)" -ForegroundColor Cyan

try {
    Copy-Item $FilePath $outputPath -Force
    
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $tempDir = [System.IO.Path]::GetTempPath() + [Guid]::NewGuid().ToString()
    [System.IO.Compression.ZipFile]::ExtractToDirectory($outputPath, $tempDir)
    
    $xmlPath = Join-Path $tempDir "word\document.xml"
    [xml]$xml = Get-Content $xmlPath -Raw
    
    # Replace with translated text (simplified)
    $xml.document.body.InnerText = $fullTranslation
    
    $xml.Save($xmlPath)
    
    Remove-Item $outputPath -Force
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $outputPath)
    Remove-Item $tempDir -Recurse -Force
    
    Write-Host "`n🎉 SUCCESS!" -ForegroundColor Green
    Write-Host "   Translated file: $outputPath" -ForegroundColor Green
    Write-Host "   Cost: ~$2-4 (Opus 4 is premium but handles large docs)" -ForegroundColor Yellow
}
catch {
    Write-Host "`n❌ Error saving DOCX: $_" -ForegroundColor Red
    
    # Fallback: Save as TXT
    $txtPath = $FilePath -replace "\.docx$", "_GREEK.txt"
    $fullTranslation | Set-Content $txtPath -Encoding UTF8
    Write-Host "   📝 Saved as TXT instead: $txtPath" -ForegroundColor Yellow
    Write-Host "   You can manually copy-paste this into a Word document" -ForegroundColor Yellow
}
