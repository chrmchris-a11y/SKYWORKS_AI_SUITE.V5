[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\\..')).Path
if (Get-Command code -ErrorAction SilentlyContinue) {
    $commands = @('github.copilot.openPanel','github.copilot-chat.open','workbench.action.openChat')
    foreach ($cmd in $commands) {
        try { Start-Process code -ArgumentList @($root,'--command', $cmd) -ErrorAction Stop; exit 0 } catch {}
    }
    try { Start-Process code -ArgumentList @($root) -ErrorAction Stop; exit 0 } catch {}
}
# Fallback: open folder if CLI missing
Start-Process $root
