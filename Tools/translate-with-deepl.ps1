param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,
    
    [Parameter(Mandatory=$false)]
    [string]$DeepLApiKey = ""  # Βάλε το API key σου εδώ
)

Write-Host "🌐 DeepL Professional Translation" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Έλεγχος API key
if ([string]::IsNullOrWhiteSpace($DeepLApiKey)) {
    Write-Host ""
    Write-Host "⚠️  DeepL API Key Required!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1️⃣  Πήγαινε στο: https://www.deepl.com/pro-api" -ForegroundColor White
    Write-Host "2️⃣  Κάνε εγγραφή (€5.49/μήνα για 500K χαρακτήρες)" -ForegroundColor White
    Write-Host "3️⃣  Πάρε το API key από Account → API Keys" -ForegroundColor White
    Write-Host "4️⃣  Τρέξε: .\translate-with-deepl.ps1 -FilePath 'FILE.docx' -DeepLApiKey 'YOUR_KEY'" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Εναλλακτικά (ΔΩΡΕΑΝ + ΓΡΗΓΟΡΟ):" -ForegroundColor Green
    Write-Host "   Χρησιμοποίησε το Microsoft Word:" -ForegroundColor Green
    Write-Host "   Review tab → Translate → Translate Document → English → Greek" -ForegroundColor Green
    Write-Host ""
    exit 1
}

# Έλεγχος αρχείου
if (-not (Test-Path $FilePath)) {
    Write-Host "❌ File not found: $FilePath" -ForegroundColor Red
    exit 1
}

$fileName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
$fileDir = [System.IO.Path]::GetDirectoryName($FilePath)
$outputPath = Join-Path $fileDir "${fileName}_GREEK.docx"

Write-Host ""
Write-Host "📄 Input:  $([System.IO.Path]::GetFileName($FilePath))" -ForegroundColor White
Write-Host "📝 Output: $([System.IO.Path]::GetFileName($outputPath))" -ForegroundColor White
Write-Host ""

# Εξαγωγή κειμένου από DOCX
Write-Host "📖 Extracting text from DOCX..." -ForegroundColor Yellow

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-DocxText {
    param([string]$DocxPath)
    
    try {
        $zip = [System.IO.Compression.ZipFile]::OpenRead($DocxPath)
        $docXmlEntry = $zip.Entries | Where-Object { $_.FullName -eq "word/document.xml" }
        
        if (-not $docXmlEntry) {
            throw "document.xml not found in DOCX"
        }
        
        $stream = $docXmlEntry.Open()
        $reader = New-Object System.IO.StreamReader($stream)
        $xmlContent = $reader.ReadToEnd()
        $reader.Close()
        $stream.Close()
        $zip.Dispose()
        
        # Parse XML και εξαγωγή text
        [xml]$xml = $xmlContent
        $namespace = @{ w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main" }
        $textNodes = Select-Xml -Xml $xml -XPath "//w:t" -Namespace $namespace
        
        $fullText = ($textNodes | ForEach-Object { $_.Node.InnerText }) -join " "
        return $fullText
        
    } catch {
        throw "Error extracting text: $_"
    }
}

$originalText = Get-DocxText -DocxPath $FilePath
$charCount = $originalText.Length

Write-Host "✅ Extracted $charCount characters" -ForegroundColor Green
Write-Host ""

# DeepL Translation
Write-Host "🌐 Translating with DeepL API..." -ForegroundColor Yellow

try {
    # Split σε chunks των 100K χαρακτήρων (DeepL limit: 128K)
    $chunkSize = 100000
    $chunks = @()
    $translatedChunks = @()
    
    for ($i = 0; $i -lt $originalText.Length; $i += $chunkSize) {
        $length = [Math]::Min($chunkSize, $originalText.Length - $i)
        $chunks += $originalText.Substring($i, $length)
    }
    
    Write-Host "✂️  Split into $($chunks.Count) chunk(s)" -ForegroundColor Cyan
    
    for ($i = 0; $i -lt $chunks.Count; $i++) {
        Write-Host "🔄 Translating chunk $($i+1)/$($chunks.Count)..." -ForegroundColor White
        
        $body = @{
            text = @($chunks[$i])
            source_lang = "EN"
            target_lang = "EL"  # Greek
            formality = "default"
            preserve_formatting = $true
        } | ConvertTo-Json
        
        $headers = @{
            "Authorization" = "DeepL-Auth-Key $DeepLApiKey"
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "https://api-free.deepl.com/v2/translate" `
                                       -Method Post `
                                       -Headers $headers `
                                       -Body $body `
                                       -TimeoutSec 120
        
        $translatedChunks += $response.translations[0].text
        Write-Host "   ✅ Chunk $($i+1) complete" -ForegroundColor Green
    }
    
    $translatedText = $translatedChunks -join "`n`n"
    
} catch {
    Write-Host "❌ DeepL API Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Έλεγξε:" -ForegroundColor Yellow
    Write-Host "   1. API key σωστό?" -ForegroundColor White
    Write-Host "   2. Έχεις credits στο DeepL account?" -ForegroundColor White
    Write-Host "   3. Χρησιμοποιείς api-free.deepl.com (free plan) ή api.deepl.com (pro plan)?" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "✅ Translation complete!" -ForegroundColor Green
Write-Host ""

# Δημιουργία νέου DOCX
Write-Host "📝 Creating translated DOCX..." -ForegroundColor Yellow

function New-TranslatedDocx {
    param(
        [string]$SourceDocxPath,
        [string]$OutputDocxPath,
        [string]$NewText
    )
    
    try {
        # Αντιγραφή του original DOCX
        Copy-Item -Path $SourceDocxPath -Destination $OutputDocxPath -Force
        
        # Άνοιγμα ως ZIP
        $zip = [System.IO.Compression.ZipFile]::Open($OutputDocxPath, [System.IO.Compression.ZipArchiveMode]::Update)
        
        # Διάβασμα του document.xml
        $docXmlEntry = $zip.Entries | Where-Object { $_.FullName -eq "word/document.xml" }
        $stream = $docXmlEntry.Open()
        $reader = New-Object System.IO.StreamReader($stream)
        $xmlContent = $reader.ReadToEnd()
        $reader.Close()
        $stream.Close()
        
        # Parse και αντικατάσταση κειμένου
        [xml]$xml = $xmlContent
        $namespace = @{ w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main" }
        $textNodes = Select-Xml -Xml $xml -XPath "//w:t" -Namespace $namespace
        
        # Καθαρισμός όλων των text nodes
        foreach ($node in $textNodes) {
            $node.Node.InnerText = ""
        }
        
        # Βάλε το μεταφρασμένο κείμενο στο πρώτο text node
        if ($textNodes.Count -gt 0) {
            $textNodes[0].Node.InnerText = $NewText
        }
        
        # Αποθήκευση
        $docXmlEntry.Delete()
        $newEntry = $zip.CreateEntry("word/document.xml")
        $newStream = $newEntry.Open()
        $writer = New-Object System.IO.StreamWriter($newStream)
        $writer.Write($xml.OuterXml)
        $writer.Close()
        $newStream.Close()
        $zip.Dispose()
        
        return $true
        
    } catch {
        Write-Host "❌ Error creating DOCX: $_" -ForegroundColor Red
        return $false
    }
}

$success = New-TranslatedDocx -SourceDocxPath $FilePath -OutputDocxPath $outputPath -NewText $translatedText

if ($success) {
    Write-Host "✅ DOCX created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Statistics:" -ForegroundColor Cyan
    Write-Host "   Original: $charCount characters" -ForegroundColor White
    Write-Host "   Translated: $($translatedText.Length) characters" -ForegroundColor White
    Write-Host "   Output: $outputPath" -ForegroundColor White
    Write-Host ""
    Write-Host "✅ COMPLETE! Opening file..." -ForegroundColor Green
    Start-Process $outputPath
} else {
    Write-Host "❌ Failed to create translated DOCX" -ForegroundColor Red
    exit 1
}
