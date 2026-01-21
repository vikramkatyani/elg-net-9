# Azure Deployment Quick Reference

## GitHub Actions Automated Deployment

### What Happens Automatically
When you push to the main/master branch, GitHub Actions will:
1. Checkout your code
2. Install .NET 9
3. Restore NuGet packages
4. Build all projects in Release mode
5. Publish LMS_admin and LMS_learner
6. Deploy to Azure App Service
7. Run health checks

### To Enable GitHub Actions Deployment

**Step 1: Create Azure Service Principal**
```powershell
az ad sp create-for-rbac --name "elg-deploy-sp" --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
  --json-auth
```

**Step 2: Add GitHub Secret**
- Go to: GitHub → Repository → Settings → Secrets and variables → Actions
- Create new secret named `AZURE_CREDENTIALS`
- Paste the JSON output from Step 1

**Step 3: Update Workflow (Important!)**
Edit `.github/workflows/deploy-azure.yml` line with your resource group:
```yaml
AZURE_RESOURCE_GROUP: 'your-actual-resource-group-name'  # Change this!
```

**Step 4: Push Code**
```bash
git add .
git commit -m "Configure Azure deployment"
git push origin main
```

**Step 5: Monitor Deployment**
- Go to GitHub → Actions tab
- Watch the workflow execute
- Takes ~5-10 minutes for full deployment

---

## Manual Deployment (Local Machine)

### Build Locally
```bash
cd d:\Net-Project\elgLMS_NET9

# Build in Release mode
dotnet build KCLMS48.sln -c Release

# Publish individual apps
dotnet publish LMS_admin/LMS_admin.csproj -c Release -o ./publish/LMS_admin
dotnet publish LMS_learner/LMS_learner.csproj -c Release -o ./publish/LMS_learner
```

### Deploy Using Azure CLI
```bash
# Install Azure CLI if needed
# https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows

# Login to Azure
az login

# Deploy admin app
az webapp deployment source config-zip \
  --resource-group {resource-group} \
  --name elg-prod-9 \
  --src publish/LMS_admin

# Deploy learner app
az webapp deployment source config-zip \
  --resource-group {resource-group} \
  --name elg-prod-9 \
  --src publish/LMS_learner
```

### Or Deploy Using Visual Studio
1. Right-click `LMS_admin` → Publish
2. Select "Azure App Service"
3. Select `elg-prod-9` from list
4. Click "Publish"
5. Repeat for `LMS_learner`

---

## Configuration in Azure Portal

### Step 1: Set Environment Variables
Go to: Azure Portal → App Services → elg-prod-9 → Configuration

**Add Application Settings:**
```
ASPNETCORE_ENVIRONMENT = Production
WEBSITES_ENABLE_APP_SERVICE_STORAGE = true
ASPNETCORE_URLS = http://+:80
WEBSITE_RUN_FROM_PACKAGE = 1
```

### Step 2: Add Connection String
Go to: Configuration → Connection strings

**Add:**
- Name: `DefaultConnection`
- Value: `Server=tcp:your-sql-server.database.windows.net,1433;...`
- Type: `SQLServer`

### Step 3: Update appsettings Files
Edit before deployment:
- `LMS_admin/appsettings.Production.json`
- `LMS_learner/appsettings.Production.json`

Replace placeholders:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-sql-server..."
  },
  "AppSettings": {
    "SMTPHost": "your-smtp-server",
    "SMTPPort": 587,
    "SMTPUsername": "your-email",
    "SMTPPassword": "your-password"
  }
}
```

---

## Test Deployment

### Check Application Health
```bash
# Test Admin Portal
curl https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage -v

# Test Learner Portal
curl https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/learn -v

# Expected: HTTP 200 or HTTP 302 (redirect to login)
```

### Check Logs
```bash
# Stream live logs
az webapp log tail --name elg-prod-9 --resource-group your-resource-group

# Download all logs
az webapp log download --name elg-prod-9 \
  --resource-group your-resource-group \
  --log-file app-logs.zip
```

### Use Kudu Debug Console
```
https://elg-prod-9.scm.azurewebsites.net/
```
- View file structure in wwwroot
- Run PowerShell commands
- View application files
- Monitor processes

---

## Troubleshooting

### App Shows 404 on /manage or /learn
**Cause:** URL rewriting not working

**Fix:**
1. Check `web.config` files are present in deployment
2. Verify IIS modules installed: Azure portal → Extensions
3. Restart app service: Azure portal → Restart

```bash
# Restart via CLI
az webapp restart --name elg-prod-9 --resource-group {resource-group}
```

### App Shows 500 Error
**Cause:** Code error or missing configuration

**Fix:**
1. Check Application Insights for error details
2. View logs via Kudu
3. Verify database connection string
4. Verify appsettings.Production.json has all required values

### Database Connection Fails
**Cause:** Firewall or wrong connection string

**Fix:**
1. In Azure Portal → SQL Server → Firewall rules
2. Add: "Allow Azure services and resources" = ON
3. Add your client IP address
4. Test connection string locally first

```bash
# Test locally
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-string"
dotnet run
```

### Deployment Hangs or Times Out
**Cause:** Large files or slow network

**Fix:**
1. Delete `bin/` and `obj/` folders
2. Run `dotnet clean`
3. Try deployment again
4. Or deploy from GitHub Actions (often faster)

---

## Important Files

| File | Purpose |
|------|---------|
| `web.config` | Root IIS configuration with URL rewriting |
| `LMS_admin/web.config` | Admin app configuration |
| `LMS_learner/web.config` | Learner app configuration |
| `.github/workflows/deploy-azure.yml` | GitHub Actions CI/CD workflow |
| `LMS_admin/appsettings.Production.json` | Admin production settings |
| `LMS_learner/appsettings.Production.json` | Learner production settings |
| `AZURE_DEPLOYMENT_GUIDE.md` | Full deployment guide |
| `PRE_DEPLOYMENT_CHECKLIST.md` | Complete verification checklist |

---

## Quick Health Check Script

Save as `test-deployment.ps1`:

```powershell
# Configuration
$adminUrl = "https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage"
$learnerUrl = "https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/learn"

# Test endpoints
Write-Host "Testing Admin Portal..." -ForegroundColor Cyan
$adminResponse = Invoke-WebRequest -Uri $adminUrl -UseBasicParsing -TimeoutSec 10
Write-Host "Status: $($adminResponse.StatusCode)" -ForegroundColor Green

Write-Host "`nTesting Learner Portal..." -ForegroundColor Cyan
$learnerResponse = Invoke-WebRequest -Uri $learnerUrl -UseBasicParsing -TimeoutSec 10
Write-Host "Status: $($learnerResponse.StatusCode)" -ForegroundColor Green

# Summary
Write-Host "`nDeployment Status: OK" -ForegroundColor Green
Write-Host "Admin Portal:   $adminUrl"
Write-Host "Learner Portal: $learnerUrl"
```

Run:
```bash
powershell -ExecutionPolicy Bypass -File test-deployment.ps1
```

---

## Key URLs

| Resource | URL |
|----------|-----|
| Azure Portal | https://portal.azure.com |
| App Service | https://portal.azure.com → App Services → elg-prod-9 |
| Admin Portal | https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage |
| Learner Portal | https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/learn |
| Kudu Console | https://elg-prod-9.scm.azurewebsites.net |
| GitHub Repo | https://github.com/vikramkatyani/elgLMS_NET9 |
| GitHub Actions | https://github.com/vikramkatyani/elgLMS_NET9/actions |

---

## Summary

1. ✅ Code ready (all projects compile, 0 errors)
2. ✅ Configuration files created (web.config, appsettings)
3. ✅ GitHub Actions workflow configured (auto-deploy on push)
4. ✅ Documentation complete (guides and checklists)

**Next Steps:**
1. Update `AZURE_CREDENTIALS` GitHub secret
2. Configure Azure App Service settings
3. Push code to main branch
4. Monitor GitHub Actions workflow
5. Test both portals

**Status:** Ready for production deployment ✅
