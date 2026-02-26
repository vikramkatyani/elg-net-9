# Step-by-step diagnostic for 413 error without Front Door

# STEP 1: Check if app service was restarted after setting WEBSITE_MAX_CONTENT_SIZE
# This is CRITICAL - the setting only takes effect after restart

Write-Host "=== 413 Error Diagnostic (No Front Door) ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "The 413 error is coming directly from Azure App Service or IIS" -ForegroundColor Yellow
Write-Host ""

# Key things to check:
Write-Host "✓ WEBSITE_MAX_CONTENT_SIZE must be set to 1073741824" -ForegroundColor Green
Write-Host "✓ App Service must be RESTARTED after setting the value" -ForegroundColor Green
Write-Host "✓ web.config must have correct requestLimits" -ForegroundColor Green
Write-Host "✓ Program.cs must configure KestrelServerOptions" -ForegroundColor Green
Write-Host ""

Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "1. Go to Azure Portal → App Service (elg-prod)" -ForegroundColor Cyan
Write-Host "2. Click Configuration → Application settings" -ForegroundColor Cyan
Write-Host "3. Verify WEBSITE_MAX_CONTENT_SIZE exists and = 1073741824" -ForegroundColor Cyan
Write-Host "4. If setting looks correct, click the App Service name at top" -ForegroundColor Cyan
Write-Host "5. Click RESTART button" -ForegroundColor Cyan
Write-Host "6. Wait 2-3 minutes for restart to complete" -ForegroundColor Cyan
Write-Host "7. Test the upload again" -ForegroundColor Cyan
Write-Host ""

Write-Host "If still getting 413 after restart:" -ForegroundColor Red
Write-Host "- Check if there's a limit in your slotName settings" -ForegroundColor Yellow
Write-Host "- Check if app has a web.config override" -ForegroundColor Yellow
Write-Host "- Look at Azure App Service Logs for details" -ForegroundColor Yellow
