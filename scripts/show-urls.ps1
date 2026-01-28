# Show Cloudflare Tunnel URLs
# This script reads tunnel logs and displays/saves the URLs

param(
    [string]$LogsDir = "..\logs",
    [string]$OutputFile = "..\cloudflare-urls.txt",
    [switch]$SaveToFile = $true
)

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$LogsDir = Join-Path $scriptDir $LogsDir
$OutputFile = Join-Path $scriptDir $OutputFile

# Read log files
$backendLog = Get-Content "$LogsDir\tunnel-backend.log" -ErrorAction SilentlyContinue
$reactLog = Get-Content "$LogsDir\tunnel-react.log" -ErrorAction SilentlyContinue
$mvcLog = Get-Content "$LogsDir\tunnel-mvc.log" -ErrorAction SilentlyContinue

# Extract URLs using regex
$urlPattern = 'https://[a-z0-9-]+\.trycloudflare\.com'
$backendUrl = ($backendLog | Select-String -Pattern $urlPattern | Select-Object -First 1).Matches.Value
$reactUrl = ($reactLog | Select-String -Pattern $urlPattern | Select-Object -First 1).Matches.Value
$mvcUrl = ($mvcLog | Select-String -Pattern $urlPattern | Select-Object -First 1).Matches.Value

# Display URLs
Write-Host ""
Write-Host "================================================================" -ForegroundColor White
Write-Host "  CLOUDFLARE PUBLIC URLS" -ForegroundColor White
Write-Host "================================================================" -ForegroundColor White
Write-Host ""

Write-Host "  [Backend API - Port 5264]" -ForegroundColor Cyan
if ($backendUrl) {
    Write-Host "  $backendUrl" -ForegroundColor Green
} else {
    Write-Host "  (Still starting... wait a moment and try again)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "  [React Admin - Port 5173] <-- Share this to interviewer!" -ForegroundColor Cyan
if ($reactUrl) {
    Write-Host "  $reactUrl" -ForegroundColor Green
} else {
    Write-Host "  (Still starting... wait a moment and try again)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "  [MVC Portal - Port 5066] <-- Share this to interviewer!" -ForegroundColor Cyan
if ($mvcUrl) {
    Write-Host "  $mvcUrl" -ForegroundColor Green
} else {
    Write-Host "  (Still starting... wait a moment and try again)" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "================================================================" -ForegroundColor White
Write-Host ""

# Save to file if requested
if ($SaveToFile -and ($backendUrl -or $reactUrl -or $mvcUrl)) {
    $date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $content = @"
=============================================================
  NGO System - Cloudflare Tunnel URLs
  Last Updated: $date
=============================================================

CLOUDFLARE PUBLIC URLS:

  [Backend API - Port 5264]
  URL: $backendUrl

  [React Admin - Port 5173] <-- Share this to interviewer!
  URL: $reactUrl

  [MVC Portal - Port 5066] <-- Share this to interviewer!
  URL: $mvcUrl

=============================================================
LOCAL ADDRESSES (Always the same):

  Backend API:       http://localhost:5264
  React Admin:       http://localhost:5173
  MVC User Portal:   http://localhost:5066

=============================================================
CONFIG UPDATES NEEDED:

  1. frontend\.env.development
     VITE_API_BASE_URL=$backendUrl/api

  2. mvc-frontend\NGOPlatformWeb\appsettings.json
     "NgrokUrl": "$mvcUrl"

=============================================================
TEST ACCOUNTS:

  React Admin:  admin@ngo.org / Admin123!
  MVC Portal:   test.user@example.com / Test123!
  ECPay Card:   4311-9511-1111-1111, 12/25, 222

=============================================================
"@

    $content | Out-File -FilePath $OutputFile -Encoding UTF8
    Write-Host "  URLs saved to: cloudflare-urls.txt" -ForegroundColor Gray
    Write-Host ""
}
