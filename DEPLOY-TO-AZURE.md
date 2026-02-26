# CRITICAL: Deploy Your Code Changes to Azure

## YOUR SITUATION:
# ✓ Code changes made locally (RequestSizeLimit attributes added)
# ✓ WEBSITE_MAX_CONTENT_SIZE set in Azure Portal (but app may need restart)
# ✗ CODE NOT YET DEPLOYED to Azure (likely the problem!)

## SOLUTION: Deploy the updated code

## Option A: Deploy using Visual Studio (Recommended)
# 1. Open ELG.Web project in Visual Studio
# 2. Right-click ELG.Web project → "Publish..."
# 3. Select your Azure App Service profile
# 4. Click "Publish"
# 5. Wait for deployment to complete (watch the Output window)
# 6. Verify: Should see "Publish succeeded"

## Option B: Deploy using Azure CLI (Terminal)
# 1. Build the project:
cd d:\Net-Project\elgLMS_NET9
dotnet build ELG.Web/ELG.Web.csproj -c Release

# 2. Publish to a local folder:
dotnet publish ELG.Web/ELG.Web.csproj -c Release -o ./publish

# 3. Deploy to Azure using ZIP deploy:
az webapp deployment source config-zip `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod `
  --src ./publish.zip

## Option C: Use Visual Studio Code (with Azure App Service extension)
# 1. Install "Azure App Service" extension
# 2. In VS Code explorer, right-click publish folder
# 3. Select "Deploy to App Service"
# 4. Select your app service

## AFTER DEPLOYMENT:
# 1. WAIT 2-3 minutes for app to restart
# 2. CLEAR YOUR BROWSER CACHE (Ctrl+Shift+Delete)
# 3. TEST THE UPLOAD

## IF STILL GETTING 413 AFTER DEPLOYMENT + RESTART:
# 1. Check App Service Logs:
#    Go to Azure Portal → elg-prod → Log stream
# 2. Look for error messages related to request size
# 3. May need to check if there's an IIS rewrite rule or WAF rule
