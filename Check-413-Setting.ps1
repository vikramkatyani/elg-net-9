# Test script to diagnose 413 error

# Check 1: Verify Azure setting is saved and get exact value
Write-Host "=== Checking WEBSITE_MAX_CONTENT_SIZE ===" -ForegroundColor Cyan

$resourceGroup = "atf-prod-core-infra-rg"
$appName = "elg-prod"

# Method 1: Using Azure CLI
$output = az webapp config appsettings list `
  --resource-group $resourceGroup `
  --name $appName `
  --query "[?name=='WEBSITE_MAX_CONTENT_SIZE'] | [0]" `
  --output json

Write-Host "Current setting from Azure:" -ForegroundColor Yellow
Write-Host $output
Write-Host ""

if ($null -eq $output -or $output -eq "") {
    Write-Host "ERROR: WEBSITE_MAX_CONTENT_SIZE is NOT SET!" -ForegroundColor Red
    Write-Host "You must set this in Azure Portal:" -ForegroundColor Red
    Write-Host "1. Go to Azure Portal > App Service > Configuration" -ForegroundColor Red
    Write-Host "2. Add new setting: WEBSITE_MAX_CONTENT_SIZE = 1073741824" -ForegroundColor Red
    Write-Host "3. Click Save and Restart" -ForegroundColor Red
} else {
    $parsed = $output | ConvertFrom-Json
    Write-Host "✓ Setting EXISTS" -ForegroundColor Green
    Write-Host "  Name: $($parsed.name)" -ForegroundColor Green
    Write-Host "  Value: $($parsed.value)" -ForegroundColor Green
    Write-Host "  Slotting: $($parsed.slotSetting)" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== What file size are you uploading? ===" -ForegroundColor Cyan
Write-Host "Answer these questions:" -ForegroundColor Yellow
Write-Host "1. File size (in MB or GB):" -ForegroundColor Yellow
Write-Host "2. Expected or actual?" -ForegroundColor Yellow
Write-Host ""
Write-Host "Note: Form data can add 20-30% overhead, so:" -ForegroundColor Yellow
Write-Host "  - 700 MB file × 1.3 = 910 MB total (OK)" -ForegroundColor Yellow
Write-Host "  - 800 MB file × 1.3 = 1040 MB total (EXCEEDS 1 GB - FAILS!)" -ForegroundColor Yellow
