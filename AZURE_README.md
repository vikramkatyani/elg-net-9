# LMS .NET 9 - Azure Deployment Infrastructure

This directory contains all necessary files and configuration for deploying the LMS (Learning Management System) to Azure App Service.

## 📁 Directory Structure

```
elgLMS_NET9/
├── LMS_Model/                          # Shared models (no changes for deployment)
├── LMS_DAL/                            # Data Access Layer
├── LMS_admin/                          # Administrator portal (.NET 9)
│   ├── appsettings.json               # Development settings
│   ├── appsettings.Production.json    # Production settings (UPDATE THIS)
│   ├── web.config                     # IIS configuration
│   └── Program.cs                     # Application startup
├── LMS_learner/                        # Learner portal (.NET 9)
│   ├── appsettings.json               # Development settings
│   ├── appsettings.Production.json    # Production settings (UPDATE THIS)
│   ├── web.config                     # IIS configuration
│   └── Program.cs                     # Application startup
├── .github/
│   └── workflows/
│       └── deploy-azure.yml           # GitHub Actions CI/CD workflow
├── web.config                         # Root IIS URL rewrite configuration
│
├── DEPLOYMENT_READY.md                # Status and summary (START HERE)
├── DEPLOYMENT_QUICK_START.md          # Quick reference guide
├── PRE_DEPLOYMENT_CHECKLIST.md        # Complete verification checklist
├── AZURE_DEPLOYMENT_GUIDE.md          # Comprehensive deployment guide
├── Verify-Deployment.ps1              # Automated verification script
├── MIGRATION_REPORT.md                # .NET 9 migration details
├── TECHNICAL_SUMMARY.md               # API compatibility reference
├── QUICK_REFERENCE.md                 # 2-page reference card
│
└── README.md                          # This file
```

## 🚀 Quick Start

### For First-Time Deployment

1. **Read the Status Document:**
   ```bash
   Open: DEPLOYMENT_READY.md
   ```

2. **Run the Verification Script:**
   ```powershell
   .\Verify-Deployment.ps1 -BuildRelease -PublishApps
   ```

3. **Follow the Checklist:**
   ```bash
   Open: PRE_DEPLOYMENT_CHECKLIST.md
   Check each item
   ```

4. **Deploy to Azure:**
   - Option A (Recommended): Use GitHub Actions (automatic on git push)
   - Option B: Manual deployment from Visual Studio
   - Option C: Use Azure CLI commands

### For Quick Reference

During deployment, refer to: **DEPLOYMENT_QUICK_START.md**

## 📋 Key Files Explained

### Configuration Files

| File | Purpose | Status |
|------|---------|--------|
| `web.config` | Root IIS configuration with URL rewrite rules | ✅ Ready |
| `LMS_admin/web.config` | Admin app IIS configuration | ✅ Ready |
| `LMS_learner/web.config` | Learner app IIS configuration | ✅ Ready |
| `LMS_admin/appsettings.Production.json` | Admin production settings | ⚠️ Update connection string |
| `LMS_learner/appsettings.Production.json` | Learner production settings | ⚠️ Update connection string |

### Documentation Files

| File | Purpose | Read Time |
|------|---------|-----------|
| **DEPLOYMENT_READY.md** | Complete summary and implementation details | 10 min |
| **DEPLOYMENT_QUICK_START.md** | Quick reference with common commands | 5 min |
| **PRE_DEPLOYMENT_CHECKLIST.md** | Step-by-step verification before deployment | 20 min |
| **AZURE_DEPLOYMENT_GUIDE.md** | Comprehensive guide with troubleshooting | 30 min |
| **MIGRATION_REPORT.md** | .NET 4.8 → .NET 9 migration details | 15 min |
| **TECHNICAL_SUMMARY.md** | API changes and compatibility reference | 15 min |
| **QUICK_REFERENCE.md** | 2-page executive summary | 3 min |

### Automation Files

| File | Purpose | Usage |
|------|---------|-------|
| `.github/workflows/deploy-azure.yml` | GitHub Actions CI/CD workflow | Auto-triggered on push |
| `Verify-Deployment.ps1` | Local verification script | Run before deployment |

## 🔧 Configuration Checklist

Before deploying, update these files:

### 1. **appsettings.Production.json** (Both apps)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:YOUR_SQL_SERVER.database.windows.net,1433;Initial Catalog=new_elg_data;..."
  },
  "AppSettings": {
    "SMTPHost": "your-smtp-server",
    "SMTPUsername": "your-email@domain.com",
    "SMTPPassword": "your-password"
  }
}
```

### 2. **.github/workflows/deploy-azure.yml**
Update line with your resource group:
```yaml
AZURE_RESOURCE_GROUP: 'your-actual-resource-group-name'
```

### 3. **GitHub Secrets** (Azure Portal)
Add in GitHub → Settings → Secrets:
- `AZURE_CREDENTIALS` - Service principal JSON

## 📊 Deployment Architecture

```
GitHub Repository (main branch)
        ↓
[Push to GitHub]
        ↓
GitHub Actions Triggered
        ↓
Build Phase (dotnet build)
  ├─ Restore packages
  ├─ Build LMS_Model
  ├─ Build LMS_DAL
  ├─ Build LMS_admin
  └─ Build LMS_learner
        ↓
Publish Phase (dotnet publish)
  ├─ Publish LMS_admin
  └─ Publish LMS_learner
        ↓
Deploy Phase
  ├─ Upload to Azure App Service
  ├─ Deploy web.config files
  ├─ Copy application files
  └─ Restart app service
        ↓
Verify Phase
  ├─ Test /manage endpoint
  └─ Test /learn endpoint
        ↓
Running on Azure
  elg-prod-9.azurewebsites.net
  ├─ /manage → LMS_admin
  └─ /learn  → LMS_learner
```

## ✅ Deployment Status

| Component | Status | Notes |
|-----------|--------|-------|
| .NET 9 Migration | ✅ Complete | All 4 projects migrated, 0 errors |
| Web Configuration | ✅ Ready | IIS URL rewrite rules configured |
| GitHub Actions | ✅ Ready | Workflow created, ready to deploy |
| Documentation | ✅ Complete | 7 comprehensive guides |
| Verification Script | ✅ Ready | Automated validation available |
| Production Settings | ⚠️ Pending | Update connection strings |
| Azure Secrets | ⚠️ Pending | Create GitHub secret with credentials |

## 🎯 Deployment Endpoints

After deployment, access applications at:

```
Admin Portal:    https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage
Learner Portal:  https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/learn
```

## 🐛 Troubleshooting

### Issue: 404 on /manage or /learn
**Solution:** Check web.config URL rewrite rules are deployed

### Issue: 500 Internal Server Error
**Solution:** Verify database connection string and appsettings.json

### Issue: Static files not loading (CSS/JS)
**Solution:** Ensure wwwroot paths use relative URLs or ASP.NET tag helpers

### For detailed troubleshooting:
See **AZURE_DEPLOYMENT_GUIDE.md** → Troubleshooting Section

## 📞 Support Resources

- **Microsoft Docs:** https://learn.microsoft.com/en-us/azure/app-service/
- **GitHub Actions:** https://docs.github.com/en/actions
- **.NET 9:** https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9
- **Entity Framework 6:** https://learn.microsoft.com/en-us/ef/ef6/

## 📅 Deployment Timeline

```
Week 1:
  Mon: Review documentation
  Tue: Run verification script
  Wed: Update configuration files
  Thu: Test locally in Release mode
  Fri: Deploy to Azure (GitHub Actions)

Post-Deployment:
  Day 1: Monitor for errors
  Week 1: User testing
  Ongoing: Performance monitoring
```

## 🔐 Security Notes

- All sensitive data should be in Azure Key Vault or App Settings
- Never commit passwords or API keys
- `.gitignore` is configured to prevent secret commits
- HTTPS enforced on all connections
- Security headers configured in web.config

## 📝 Version Information

- **Framework:** .NET 9.0
- **Previous:** .NET Framework 4.8
- **SQL Server:** new_elg_data database
- **Repository:** https://github.com/vikramkatyani/elgLMS_NET9
- **Deployment:** Azure App Service (elg-prod-9)

## 🎓 Learning Resources

This deployment uses:
- ✅ GitHub Actions for CI/CD
- ✅ Azure App Service for hosting
- ✅ IIS URL Rewrite for routing
- ✅ Entity Framework 6 for data access
- ✅ ASP.NET Core dependency injection
- ✅ Application Insights for monitoring

Perfect for learning modern cloud deployment practices!

---

**Status:** Ready for Production Deployment ✅

**Next Action:** Open [DEPLOYMENT_READY.md](DEPLOYMENT_READY.md)
