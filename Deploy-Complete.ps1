# Complete deployment script for ELG.Web
# This script publishes to Release and deploys to Azure

$ErrorActionPreference = "Stop"

$workspaceFolder = "D:\Net-Project\elgLMS_NET9"
$resourceGroup = "atf-prod-core-infra-rg"
$appServiceName = "elg-prod"

Write-Host "=== ELG.Web Deployment Script ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Publish
Write-Host "Step 1: Publishing application..." -ForegroundColor Yellow
cd $workspaceFolder
dotnet publish ELG.Web/ELG.Web.csproj -c Release -o ./publish --no-build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Publish successful" -ForegroundColor Green
Write-Host ""

# Step 2: Create ZIP
Write-Host "Step 2: Creating ZIP package..." -ForegroundColor Yellow
Compress-Archive -Path "./publish/*" -DestinationPath "./publish.zip" -Force

if (Test-Path ./publish.zip) {
    $zipSize = (Get-Item ./publish.zip).Length / 1MB
    Write-Host "✓ ZIP created: $([Math]::Round($zipSize, 2)) MB" -ForegroundColor Green
} else {
    Write-Host "ERROR: ZIP creation failed!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Deploy to Azure
Write-Host "Step 3: Deploying to Azure App Service..." -ForegroundColor Yellow
Write-Host "Resource Group: $resourceGroup" -ForegroundColor Gray
Write-Host "App Service: $appServiceName" -ForegroundColor Gray
Write-Host ""

az webapp deploy `
  --resource-group $resourceGroup `
  --name $appServiceName `
    --src-path ./publish.zip `
    --type zip

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Azure deployment failed!" -ForegroundColor Red
    Write-Host "Make sure you've run: az login" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "✓ Deployment successful!" -ForegroundColor Green
Write-Host ""
Write-Host "Step 4: Waiting for app service to restart..." -ForegroundColor Yellow
Write-Host "This typically takes 2-3 minutes..." -ForegroundColor Gray
Write-Host ""

Start-Sleep -Seconds 180

Write-Host "✓ App service should be restarted" -ForegroundColor Green
Write-Host ""
Write-Host "=== TESTING ===" -ForegroundColor Cyan
Write-Host "Open your browser and test:" -ForegroundColor Yellow
Write-Host "https://app.elearningate.com/CourseManagement/UploadScormPackage" -ForegroundColor Cyan
Write-Host ""
Write-Host "Try uploading your 153 MB file" -ForegroundColor Yellow
Write-Host ""
Write-Host "If still getting 413:" -ForegroundColor Red
Write-Host "1. Open DevTools (F12) → Network tab" -ForegroundColor Yellow
Write-Host "2. Look at response headers for clues" -ForegroundColor Yellow
Write-Host "3. Check if error is from IIS or application" -ForegroundColor Yellow
Write-Host ""
