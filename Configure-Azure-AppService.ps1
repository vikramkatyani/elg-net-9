# Azure App Service Configuration Script for Large File Uploads
# This script configures the WEBSITE_MAX_CONTENT_SIZE setting which is required
# for uploads larger than 30MB on Azure App Service

# Replace these values with your actual Azure resource information
$resourceGroup = "your-resource-group"          # e.g., "elg-prod-rg"
$appServiceName = "your-app-service-name"       # e.g., "elg-prod-manage"
$subscriptionId = "your-subscription-id"        # Optional: if you have multiple subscriptions

# Size in bytes (1 GB)
$maxContentSize = 1073741824

# Authenticate to Azure (if not already authenticated)
# Uncomment the line below if needed
# Connect-AzAccount

# Select subscription if provided
if ($subscriptionId) {
    Select-AzSubscription -SubscriptionId $subscriptionId
}

Write-Host "Configuring Azure App Service: $appServiceName" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroup" -ForegroundColor Cyan
Write-Host "Max Content Size: $maxContentSize bytes (1 GB)" -ForegroundColor Green

# Get the app service
$appService = Get-AzWebApp -ResourceGroupName $resourceGroup -Name $appServiceName
if (-not $appService) {
    Write-Error "App Service not found: $appServiceName in resource group $resourceGroup"
    exit 1
}

# Update app settings
Write-Host "Updating WEBSITE_MAX_CONTENT_SIZE setting..." -ForegroundColor Yellow

$appSettings = @{}
# Copy existing app settings
foreach ($setting in $appService.SiteConfig.AppSettings) {
    $appSettings[$setting.Name] = $setting.Value
}

# Set/update the WEBSITE_MAX_CONTENT_SIZE
$appSettings["WEBSITE_MAX_CONTENT_SIZE"] = $maxContentSize.ToString()

# Apply settings
Set-AzWebApp -ResourceGroupName $resourceGroup -Name $appServiceName -AppSettings $appSettings

Write-Host "✓ WEBSITE_MAX_CONTENT_SIZE has been set to $maxContentSize bytes" -ForegroundColor Green

# Restart the app service
Write-Host "Restarting App Service to apply changes..." -ForegroundColor Yellow
Restart-AzWebApp -ResourceGroupName $resourceGroup -Name $appServiceName

Write-Host "✓ App Service has been restarted" -ForegroundColor Green
Write-Host "Configuration complete! Large file uploads should now work." -ForegroundColor Green

# Verify the setting
Write-Host "`nVerifying configuration..." -ForegroundColor Cyan
$updatedApp = Get-AzWebApp -ResourceGroupName $resourceGroup -Name $appServiceName
$setting = $updatedApp.SiteConfig.AppSettings | Where-Object { $_.Name -eq "WEBSITE_MAX_CONTENT_SIZE" }
if ($setting) {
    Write-Host "✓ Verified: WEBSITE_MAX_CONTENT_SIZE = $($setting.Value)" -ForegroundColor Green
} else {
    Write-Host "⚠ Setting not found after update" -ForegroundColor Yellow
}
