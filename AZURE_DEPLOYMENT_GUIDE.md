# Azure App Service Deployment Guide

## Quick Summary

This guide covers deploying the LMS_admin and LMS_learner applications to Azure App Service with the following routing:
- **Admin**: `https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage`
- **Learner**: `https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/learn`

---

## Prerequisites

1. **Azure Subscription** - Active Azure account with:
   - App Service Plan (already created: `elg-prod-9`)
   - SQL Server/Database (new_elg_data database)
   - Storage Account (for file uploads, optional)

2. **GitHub Repository** - Access to https://github.com/vikramkatyani/elgLMS_NET9

3. **Required Secrets** in GitHub:
   - `AZURE_CREDENTIALS` - Azure service principal credentials
   - Database connection string
   - SMTP credentials (email settings)
   - SAML configuration (if applicable)

---

## Automated Deployment (Recommended)

### Option 1: GitHub Actions CI/CD

The workflow is configured in `.github/workflows/deploy-azure.yml` and will:
- Automatically build and test on every push
- Deploy to Azure App Service on push to main/master branches
- Run health checks post-deployment

**Setup Steps:**

1. **Configure Azure Credentials:**
   ```powershell
   # Run this in Azure CLI or Azure Portal
   az ad sp create-for-rbac --name "elg-deploy" --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
     --json-auth
   ```

2. **Add GitHub Secret:**
   - Go to GitHub repo → Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `AZURE_CREDENTIALS`
   - Value: Paste the JSON output from the command above

3. **Update Workflow File:**
   Edit `.github/workflows/deploy-azure.yml`:
   ```yaml
   AZURE_RESOURCE_GROUP: 'your-actual-resource-group'  # Update this line
   ```

4. **Push Changes:**
   ```bash
   git add .github/workflows/deploy-azure.yml
   git commit -m "Configure Azure deployment workflow"
   git push origin main
   ```

5. **Monitor Deployment:**
   - Go to GitHub repo → Actions tab
   - Watch the workflow execute
   - Deployment takes approximately 5-10 minutes

---

## Manual Deployment

### Option 2: Direct Publishing from Local Machine

If you prefer to deploy manually without GitHub Actions:

**Using Visual Studio:**
1. Right-click on `LMS_admin` project → Publish
2. Select "Azure App Service" as target
3. Select existing App Service `elg-prod-9`
4. Configure settings and publish

**Using .NET CLI:**

```bash
# Build Release configuration
dotnet build LMS_admin/LMS_admin.csproj -c Release
dotnet build LMS_learner/LMS_learner.csproj -c Release

# Publish
dotnet publish LMS_admin/LMS_admin.csproj -c Release -o ./publish/LMS_admin
dotnet publish LMS_learner/LMS_learner.csproj -c Release -o ./publish/LMS_learner

# Deploy using Azure CLI
az webapp up --name elg-prod-9 --resource-group your-resource-group --plan your-app-service-plan
```

---

## Configuration Required Before Deployment

### 1. Update Connection Strings

**In both `appsettings.Production.json` files:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-sql-server.database.windows.net,1433;Initial Catalog=new_elg_data;Persist Security Info=False;User ID=your-username;Password=your-password;..."
  }
}
```

**Or set as Azure App Service Environment Variables:**

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=tcp:...
```

### 2. Configure App Service Settings

In Azure Portal → App Service → Configuration:

**Add these Application Settings:**

| Key | Value | Notes |
|-----|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Enables Production config files |
| `WEBSITES_ENABLE_APP_SERVICE_STORAGE` | `true` | Enable local file storage |
| `ASPNETCORE_URLS` | `http://+:80` | Kestrel binding |
| `WEBSITE_RUN_FROM_PACKAGE` | `1` | Run from deployment package (recommended) |

**Add Connection Strings:**

| Name | Value | Type |
|------|-------|------|
| `DefaultConnection` | `Server=tcp:...` | SQLServer |

### 3. Database Migrations

If using Entity Framework migrations:

```bash
# Before deployment, ensure database is updated
dotnet ef database update --project LMS_DAL/LMS_DAL.csproj

# Or run migrations in App Service using Kudu:
# https://elg-prod-9.scm.azurewebsites.net/
```

---

## Post-Deployment Verification

### Health Checks

Test the endpoints to ensure everything is working:

```powershell
# Test Admin Portal
Invoke-WebRequest -Uri "https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage" -UseBasicParsing

# Test Learner Portal
Invoke-WebRequest -Uri "https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/learn" -UseBasicParsing

# Check logs (requires Kudu/SSH)
# https://elg-prod-9.scm.azurewebsites.net/
```

### Checklist

- [ ] Both URLs return HTTP 200 (or redirect to login)
- [ ] Login functionality works
- [ ] Database queries return data
- [ ] File uploads work correctly
- [ ] SAML authentication works (if configured)
- [ ] Static assets load (CSS, JS)
- [ ] No 500 errors in logs
- [ ] Application Insights shows data (if configured)

---

## Troubleshooting

### Issue: 404 Not Found on /manage or /learn

**Solution:**
1. Check `web.config` files are deployed correctly
2. Verify IIS URL Rewrite module is installed on App Service
3. Review rewrite rules in root `web.config`

```powershell
# Check deployment via Kudu
# https://elg-prod-9.scm.azurewebsites.net/
# Navigate to wwwroot to verify file structure
```

### Issue: 500 Internal Server Error

**Solutions:**
1. Enable detailed error pages:
   ```xml
   <!-- In web.config -->
   <system.webServer>
     <httpErrors errorMode="Detailed" />
   </system.webServer>
   ```

2. Check Application Insights for exceptions
3. Review Azure App Service logs:
   ```bash
   az webapp log tail --name elg-prod-9 --resource-group your-resource-group
   ```

### Issue: Database Connection Failed

**Solutions:**
1. Verify connection string is correct
2. Check SQL Server firewall rules allow Azure App Service
3. Test connection string locally first:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
   ```

### Issue: Static Files Not Loading (CSS/JS)

**Solutions:**
1. Ensure `web.config` static file caching is enabled
2. Check file paths in views (should be relative or use `asp-append-version`)
3. Configure CDN if serving from sub-path

```html
<!-- In views, use path helpers -->
<link rel="stylesheet" href="~/Content/styles.css" asp-append-version="true" />
```

---

## Performance Optimization

### 1. Enable Caching

Already configured in `web.config`:
```xml
<staticContent>
  <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="365.00:00:00" />
</staticContent>
```

### 2. Enable Compression

Already configured in `web.config`:
```xml
<urlCompression doDynamicCompression="true" doStaticCompression="true" />
```

### 3. Configure Auto-Scale (Optional)

In Azure Portal:
1. App Service Plan → Scale Out (App Service Plan)
2. Set Min instances: 2, Max instances: 10
3. Configure autoscale based on CPU or memory

### 4. Enable Application Insights

```bash
# Enable monitoring
az monitor app-insights component create \
  --app elg-prod-9-insights \
  --location uksouth \
  --resource-group your-resource-group
```

Then update `appsettings.Production.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

---

## Rollback Procedure

If deployment causes issues:

### Using Azure Portal:
1. Go to App Service → Deployments → Deployment slots
2. Select previous deployment
3. Click "Swap" to rollback

### Using Azure CLI:
```bash
# View deployment history
az webapp deployment list --name elg-prod-9 --resource-group your-resource-group

# Restore to specific deployment
az webapp deployment slot swap --name elg-prod-9 \
  --resource-group your-resource-group \
  --slot staging
```

---

## Maintenance & Monitoring

### Regular Tasks

1. **Weekly:**
   - Check Application Insights for errors
   - Monitor CPU and memory usage
   - Review user access logs

2. **Monthly:**
   - Update NuGet packages for security patches
   - Verify backups are running
   - Check disk space usage

3. **Quarterly:**
   - Full security assessment
   - Performance review and optimization
   - Database maintenance and indexing

### Log Access

```bash
# Stream live logs
az webapp log tail --name elg-prod-9 --resource-group your-resource-group

# Download logs
az webapp log download --name elg-prod-9 \
  --resource-group your-resource-group \
  --log-file ~/downloads/app-logs.zip
```

---

## Support Resources

- **Azure App Service Docs:** https://learn.microsoft.com/en-us/azure/app-service/
- **ASP.NET Core on Azure:** https://learn.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore
- **GitHub Actions:** https://docs.github.com/en/actions
- **Entity Framework 6:** https://learn.microsoft.com/en-us/ef/ef6/

---

**Last Updated:** 2024
**Status:** Ready for Production Deployment
