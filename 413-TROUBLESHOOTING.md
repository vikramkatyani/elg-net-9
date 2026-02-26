# Troubleshooting 413 Request Entity Too Large on Azure
# For app.elearningate.com endpoint

## STEP 1: Verify Azure App Service Setting via Azure CLI
## Run this in PowerShell or any terminal with Azure CLI:

az webapp config appsettings list `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod `
  --query "[?name=='WEBSITE_MAX_CONTENT_SIZE']"

# Expected output:
# [
#   {
#     "name": "WEBSITE_MAX_CONTENT_SIZE",
#     "value": "1073741824"
#   }
# ]

# If it's NOT showing or value is wrong, set it:
az webapp config appsettings set `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod `
  --settings WEBSITE_MAX_CONTENT_SIZE=1073741824

# Then restart:
az webapp restart `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod


## STEP 2: Check for Azure Front Door
## Since your domain is app.elearningate.com (not app.azurewebsites.net),
## you're likely behind Azure Front Door.
## This is the MOST LIKELY cause of your 413 error!

# Check if Front Door exists:
az afd profile list --resource-group atf-prod-core-infra-rg

# If Front Door is configured, you need to increase its body size limit:
# The default is 30 MB on Azure Front Door (very restrictive!)


## STEP 3: Check for Application Gateway or Azure WAF

az network application-gateway list `
  --resource-group atf-prod-core-infra-rg

# Check WAF policies:
az network waf-policy list `
  --resource-group atf-prod-core-infra-rg


## STEP 4: Check what's behind app.elearningate.com

# Run DNS lookup to see what app.elearningate.com points to:
nslookup app.elearningate.com

# If it points to something like "*.azureedge.net", that's Azure CDN/Front Door
# If it points to something like "*.trafficmanager.net", that's Traffic Manager


## SOLUTIONS by Configuration Type:

### IF BEHIND AZURE FRONT DOOR (Most Likely):
### ==============================================

# 1. Go to Azure Portal
# 2. Search for "Front Doors and CDN profiles"
# 3. Find your Front Door profile
# 4. Go to Settings > Rules Engine
#    OR go to Backend pool settings
# 5. Look for "Max request body size" or similar setting
# 6. Increase to at least 1 GB
# 7. Save and wait for deployment (2-5 minutes)

# Via CLI (if supported):
az afd rule-set rule create `
  --profile-name <your-front-door-name> `
  --resource-group atf-prod-core-infra-rg `
  --rule-set-name <rule-set-name> `
  --name increase-body-size


### IF BEHIND APPLICATION GATEWAY:
### ================================

# 1. Go to Azure Portal
# 2. Find your Application Gateway
# 3. Go to HTTP settings
# 4. Set "Request body size limit" to at least 1 GB
# 5. Save and wait for update


### IF NO REVERSE PROXY:
### ====================

# If app.elearningate.com directly points to elg-prod.azurewebsites.net:
# 1. Verify WEBSITE_MAX_CONTENT_SIZE is set (see Step 1 above)
# 2. Check web.config has correct requestLimits
# 3. Verify Program.cs initializes KestrelServerOptions
# 4. Restart app service
# 5. Clear browser cache and test


## DEBUGGING:

# 1. Open browser DevTools (F12)
# 2. Go to Network tab
# 3. Try the upload
# 4. Look at the 413 response headers to see what responses with the limit
#    Examples:
#    - "azure-fd" in headers = Azure Front Door is the culprit
#    - "waf" in headers = WAF is blocking it
#    - Just normal ASP.NET response = Application issue

# 2. Check App Service logs:
az webapp log stream `
  --resource-group atf-prod-core-infra-rg `
  --name elg-prod `
  --provider Microsoft.Web

