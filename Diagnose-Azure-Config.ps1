# Diagnostic script to verify Azure App Service configuration
# Run this to troubleshoot the 413 error

$resourceGroup = "atf-prod-core-infra-rg"
$appServiceName = "elg-prod"
$subscriptionId = "c01f61bc-1f6e-40b6-ac1a-569e7d6003e7"

Write-Host "=== Azure App Service Configuration Diagnostics ===" -ForegroundColor Cyan
Write-Host ""

# Set subscription
Select-AzSubscription -SubscriptionId $subscriptionId | Out-Null

# Get app service
$appService = Get-AzWebApp -ResourceGroupName $resourceGroup -Name $appServiceName
if (-not $appService) {
    Write-Error "App Service not found"
    exit 1
}

Write-Host "App Service: $appServiceName" -ForegroundColor Green
Write-Host "Resource Group: $resourceGroup" -ForegroundColor Green
Write-Host ""

# Check WEBSITE_MAX_CONTENT_SIZE
Write-Host "1. Checking WEBSITE_MAX_CONTENT_SIZE Setting:" -ForegroundColor Yellow
$appSettings = $appService.SiteConfig.AppSettings
$websiteMaxSetting = $appSettings | Where-Object { $_.Name -eq "WEBSITE_MAX_CONTENT_SIZE" }

if ($websiteMaxSetting) {
    $sizeBytes = [long]$websiteMaxSetting.Value
    $sizeGB = $sizeBytes / 1GB
    Write-Host "   ✓ WEBSITE_MAX_CONTENT_SIZE = $($websiteMaxSetting.Value) bytes ($sizeGB GB)" -ForegroundColor Green
} else {
    Write-Host "   ✗ WEBSITE_MAX_CONTENT_SIZE is NOT SET (default 30 MB)" -ForegroundColor Red
    Write-Host "   Action: Must set this in Azure Portal or via CLI" -ForegroundColor Yellow
}

Write-Host ""

# Check if app is behind Front Door
Write-Host "2. Checking for Azure Front Door:" -ForegroundColor Yellow
$resources = Get-AzResource -ResourceGroupName $resourceGroup -ErrorAction SilentlyContinue
$frontDoors = $resources | Where-Object { $_.ResourceType -eq "Microsoft.Network/frontDoors" }

if ($frontDoors) {
    Write-Host "   ⚠ DETECTED: Azure Front Door is configured" -ForegroundColor Yellow
    foreach ($fd in $frontDoors) {
        Write-Host "     - Front Door: $($fd.Name)" -ForegroundColor Yellow
        Write-Host "     Action: Check Front Door backend settings for body size limits" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✓ No Azure Front Door detected" -ForegroundColor Green
}

Write-Host ""

# Check if app is behind Application Gateway
Write-Host "3. Checking for Application Gateway:" -ForegroundColor Yellow
$appGateways = $resources | Where-Object { $_.ResourceType -eq "Microsoft.Network/applicationGateways" }

if ($appGateways) {
    Write-Host "   ⚠ DETECTED: Application Gateway is configured" -ForegroundColor Yellow
    foreach ($ag in $appGateways) {
        Write-Host "     - Gateway: $($ag.Name)" -ForegroundColor Yellow
        Write-Host "     Action: Check Application Gateway HTTP settings for request body size limit" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✓ No Application Gateway detected" -ForegroundColor Green
}

Write-Host ""

# Check App Service Plan
Write-Host "4. Checking App Service Plan:" -ForegroundColor Yellow
$appServicePlan = Get-AzAppServicePlan -ResourceGroupName $appService.ResourceGroup -Name $appService.AppServicePlanName
Write-Host "   Plan: $($appServicePlan.Name)" -ForegroundColor Green
Write-Host "   Tier: $($appServicePlan.Sku.Name)" -ForegroundColor Green

Write-Host ""

# Check state and last modified
Write-Host "5. App Service Status:" -ForegroundColor Yellow
Write-Host "   State: $($appService.State)" -ForegroundColor Green
$lastModified = [DateTime]::Parse($appService.LastModifiedTimeUtc).ToLocalTime()
Write-Host "   Last Modified: $lastModified" -ForegroundColor Green

Write-Host ""

# Recommendations
Write-Host "=== RECOMMENDATIONS ===" -ForegroundColor Cyan

$issues = @()

if (-not $websiteMaxSetting) {
    $issues += "1. WEBSITE_MAX_CONTENT_SIZE is not set - this is blocking large uploads"
}

if ($frontDoors) {
    $issues += "2. Azure Front Door detected - may have its own size limits (default 30 MB)"
}

if ($appGateways) {
    $issues += "3. Application Gateway detected - may have its own size limits"
}

if ($appService.State -ne "Running") {
    $issues += "4. App Service is not running (State: $($appService.State))"
}

if ($issues.Count -eq 0) {
    Write-Host "Configuration appears to be correct. Check:" -ForegroundColor Green
    Write-Host "- Was the app restarted after setting WEBSITE_MAX_CONTENT_SIZE?" -ForegroundColor Yellow
    Write-Host "- Check browser Network tab to see where the 413 is coming from" -ForegroundColor Yellow
    Write-Host "- Check App Service logs for details" -ForegroundColor Yellow
} else {
    Write-Host "Issues found:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  $issue" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== NEXT STEPS ===" -ForegroundColor Cyan
Write-Host "1. Verify WEBSITE_MAX_CONTENT_SIZE is set to 1073741824" -ForegroundColor Cyan
Write-Host "2. Check if there's Azure Front Door/Application Gateway" -ForegroundColor Cyan
Write-Host "3. Restart the app service after any config changes" -ForegroundColor Cyan
Write-Host "4. Check app logs: App Service > Log stream" -ForegroundColor Cyan
