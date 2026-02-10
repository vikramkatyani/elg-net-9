# Azure Deployment Checklist - Final Verification

## Pre-Deployment Phase

### Code Quality
- [x] All projects build successfully with 0 errors
  - [x] LMS_admin
  - [x] LMS_learner
  - [x] LMS_DAL
  - [x] LMS_Model
- [x] No critical compiler warnings
- [x] Migration from .NET Framework 4.8 to .NET 9 complete
- [x] All API incompatibilities resolved
- [x] Code review completed

### Configuration Files
- [x] `web.config` files created for URL routing
  - [x] Root `web.config` with rewrite rules
  - [x] LMS_admin/web.config with app settings
  - [x] LMS_learner/web.config with app settings
- [x] `appsettings.Development.json` verified
- [x] `appsettings.Production.json` created (with placeholders)
- [x] `.github/workflows/deploy-azure.yml` created

### Database
- [x] Database connection string verified
- [x] Entity Framework migrations applicable
- [x] SQL Server firewall rules prepared
- [ ] Database backups taken (DO THIS BEFORE DEPLOYMENT)

### Security
- [x] Sensitive data removed from code
- [x] `.gitignore` configured for secrets
- [x] HTTPS redirects configured
- [x] Security headers added (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection)
- [ ] Azure Key Vault secrets configured

### Documentation
- [x] MIGRATION_REPORT.md created
- [x] DEPLOYMENT_CHECKLIST.md created
- [x] TECHNICAL_SUMMARY.md created
- [x] QUICK_REFERENCE.md created
- [x] AZURE_DEPLOYMENT_GUIDE.md created
- [x] GitHub Actions workflow documented

---

## Azure Setup Phase

### Azure Portal Configuration

#### App Service Plan
- [ ] Verify App Service Plan: `elg-prod-9`
- [ ] Check pricing tier (minimum B1 or B2 recommended for 2 apps)
- [ ] Verify region: UK South
- [ ] Enable Always On (recommended for production)
  - Settings → General Settings → Always On: ON

#### App Service Configuration
- [ ] Go to App Service: `elg-prod-9`
- [ ] Configure Environment Variables:
  ```
  ASPNETCORE_ENVIRONMENT = Production
  WEBSITES_ENABLE_APP_SERVICE_STORAGE = true
  ASPNETCORE_URLS = http://+:80
  WEBSITE_RUN_FROM_PACKAGE = 1
  ```
- [ ] Add Connection Strings:
  - Type: SQLServer
  - Name: DefaultConnection
  - Value: `Server=tcp:...` (from SQL Server)
- [ ] Configure SSL Certificate (HTTPS)
  - TLS/SSL Settings → Upload or auto-renew managed certificate

#### Database Connection
- [ ] Verify SQL Server Firewall Rules
  - Allow Azure Services: ON
  - Allow Client IP: Add your IP address
- [ ] Test connection string from local machine
- [ ] Verify database `new_elg_data` exists
- [ ] Run any pending migrations: `dotnet ef database update`

#### Application Insights (Optional but Recommended)
- [ ] Create Application Insights resource
- [ ] Get Instrumentation Key
- [ ] Update `appsettings.Production.json` with key
- [ ] Configure sampling rules if needed

### GitHub Setup

#### Repository Configuration
- [ ] Clone repository: `https://github.com/vikramkatyani/elgLMS_NET9`
- [ ] Create Azure Credentials secret:
  - Go to Settings → Secrets and variables → Actions
  - Add `AZURE_CREDENTIALS` (JSON from `az ad sp create-for-rbac`)
- [ ] Add GitHub Secrets:
  - `DATABASE_CONNECTION_STRING` (optional, can use App Service config)
  - `SMTP_PASSWORD` (if different from appsettings)
  - Other sensitive values as needed
- [ ] Verify GitHub Actions workflow file: `.github/workflows/deploy-azure.yml`
  - Update `AZURE_RESOURCE_GROUP` value
  - Verify app name: `elg-prod-9`

---

## Pre-Deployment Validation

### Local Testing
- [ ] Run both applications locally in Release mode:
  ```bash
  dotnet run --configuration Release --project LMS_admin/LMS_admin.csproj
  dotnet run --configuration Release --project LMS_learner/LMS_learner.csproj
  ```
- [ ] Test URLs:
  - http://localhost:5000 (Admin)
  - http://localhost:5001 (Learner)
- [ ] Verify database connectivity
- [ ] Test login functionality
- [ ] Test file uploads (if applicable)
- [ ] Check static assets load correctly

### Environment File Updates
- [ ] Update `appsettings.Production.json` files with:
  - [ ] Actual SQL Server connection string
  - [ ] SMTP credentials
  - [ ] Base URL: `https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net`
  - [ ] SAML configuration (if applicable)
  - [ ] Application Insights key
  - [ ] Any other environment-specific settings

### Final Code Review
- [ ] All sensitive data removed from commits
- [ ] No hardcoded passwords or API keys
- [ ] All dependencies are compatible with .NET 9
- [ ] No breaking changes from .NET Framework 4.8

---

## Deployment Phase

### Option A: GitHub Actions (Recommended)
- [ ] Push code to `main` or `master` branch
- [ ] Go to GitHub Actions tab
- [ ] Verify workflow starts automatically
- [ ] Monitor build step:
  - [ ] Dependencies restore successfully
  - [ ] Projects build without errors
  - [ ] Publish step creates deployment packages
- [ ] Monitor deploy step:
  - [ ] Login to Azure succeeds
  - [ ] Files upload to App Service
  - [ ] Service restarts
- [ ] Check post-deployment health checks
- [ ] Verify workflow completion email

### Option B: Manual Deployment (If Actions fails)
```bash
# Build
dotnet build elgLMS.sln -c Release

# Publish
dotnet publish LMS_admin/LMS_admin.csproj -c Release -o ./publish/LMS_admin
dotnet publish LMS_learner/LMS_learner.csproj -c Release -o ./publish/LMS_learner

# Deploy (via Visual Studio or Azure CLI)
az webapp deployment source config-zip \
  --resource-group your-resource-group \
  --name elg-prod-9 \
  --src published-apps.zip
```

---

## Post-Deployment Verification

### URL Routing Test
- [ ] Admin Portal: https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage
  - Expected: Login page or admin dashboard
  - Status code: 200 or 302 (redirect)
- [ ] Learner Portal: https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/learn
  - Expected: Login page or learner dashboard
  - Status code: 200 or 302 (redirect)

### Functional Testing
- [ ] Login works on both portals
- [ ] Dashboard loads without errors
- [ ] Database queries execute successfully
- [ ] User data displays correctly
- [ ] File uploads work (if applicable)
- [ ] Logout functionality works
- [ ] Navigation between pages works
- [ ] Search functionality works
- [ ] Filtering and sorting work

### Performance Testing
- [ ] Page load times acceptable (< 3 seconds)
- [ ] No 500 errors in logs
- [ ] Memory usage stable (no memory leaks)
- [ ] CPU usage reasonable
- [ ] Database connection pooling working

### Security Verification
- [ ] HTTPS enforced (redirects HTTP to HTTPS)
- [ ] Security headers present:
  - X-Content-Type-Options: nosniff
  - X-Frame-Options: SAMEORIGIN
  - X-XSS-Protection: 1; mode=block
- [ ] No sensitive data in logs
- [ ] Authentication/Authorization working
- [ ] Session management secure

### Logging & Monitoring
- [ ] Application logs available in App Service
- [ ] Application Insights shows data (if configured)
- [ ] No error events
- [ ] Request tracking working
- [ ] Exception logging functional

---

## Post-Deployment Tasks

### First 24 Hours
- [ ] Monitor application performance continuously
- [ ] Check logs for any warnings
- [ ] Verify scheduled jobs run (if applicable)
- [ ] Confirm email notifications work (if applicable)
- [ ] Test critical user journeys
- [ ] Verify database backup completed

### First Week
- [ ] Check user feedback for issues
- [ ] Verify all reports run correctly
- [ ] Test edge cases and error scenarios
- [ ] Load test with realistic user count
- [ ] Document any configuration changes
- [ ] Update team with deployment status

### Ongoing
- [ ] Set up automated monitoring alerts
- [ ] Schedule weekly performance reviews
- [ ] Plan security patches
- [ ] Document lessons learned
- [ ] Update deployment documentation

---

## Rollback Plan (If Issues Occur)

If critical issues arise:

1. **Immediate Actions:**
   - [ ] Stop accepting new requests
   - [ ] Notify stakeholders
   - [ ] Collect error logs

2. **Rollback via Azure Portal:**
   - [ ] Go to App Service → Deployments
   - [ ] Select previous successful deployment
   - [ ] Click "Restart" or "Redeploy"

3. **Rollback via GitHub:**
   - [ ] Revert problematic commit
   - [ ] Push to main branch
   - [ ] GitHub Actions redeploys previous working version

4. **Manual Rollback:**
   - [ ] Restore database from backup (if needed)
   - [ ] Deploy previous release build manually
   - [ ] Verify system functionality

5. **Post-Rollback Analysis:**
   - [ ] Identify root cause
   - [ ] Create issue/ticket
   - [ ] Fix locally and test thoroughly
   - [ ] Redeploy when ready

---

## Important Notes

### URL Rewriting
- The `web.config` files handle URL rewriting via IIS
- /manage/* routes to LMS_admin
- /learn/* routes to LMS_learner
- This is transparent to the applications

### Session Management
- Both apps share the same domain (same-site sessions)
- Cookie paths are rewritten to include /manage or /learn
- Session state should be configured in AppSettings

### Static Files
- CSS, JS, and images should be in `wwwroot` folders
- Static files are served directly without app routing
- Ensure relative paths work with sub-path URLs

### Database
- Both applications share the same database
- Ensure migrations are version-controlled
- Backup database before each deployment

### Monitoring
- Always have monitoring enabled in production
- Set up alerts for errors and high resource usage
- Review logs regularly for issues

---

## Contact & Support

- **Repository:** https://github.com/vikramkatyani/elgLMS_NET9
- **Azure Portal:** https://portal.azure.com
- **App Service:** https://elg-prod-9.azurewebsites.net
- **Kudu Console:** https://elg-prod-9.scm.azurewebsites.net

---

**Deployment Date:** [Fill in when deploying]
**Deployed By:** [Your name]
**Version Deployed:** [Git commit hash]

**Status:** ✅ READY FOR DEPLOYMENT
