# Azure Deployment Flow Diagram

## 1️⃣ Pre-Deployment Phase

```
┌─────────────────────────────────────────────────────┐
│          PRE-DEPLOYMENT PREPARATION                │
└─────────────────────────────────────────────────────┘
        │
        ├──► 1. Review Documentation
        │        ├─ DEPLOYMENT_READY.md ✓
        │        ├─ AZURE_README.md ✓
        │        └─ DEPLOYMENT_QUICK_START.md ✓
        │
        ├──► 2. Update Configuration Files
        │        ├─ appsettings.Production.json (LMS_admin)
        │        │   └─ Add: Connection String, SMTP, App Insights Key
        │        └─ appsettings.Production.json (LMS_learner)
        │            └─ Add: Connection String, SMTP, SAML Config
        │
        ├──► 3. Update GitHub Actions Workflow
        │        └─ .github/workflows/deploy-azure.yml
        │            └─ Set: AZURE_RESOURCE_GROUP
        │
        ├──► 4. Create GitHub Secrets
        │        └─ Settings → Secrets → Actions
        │            └─ Add: AZURE_CREDENTIALS (JSON from Azure Portal)
        │
        ├──► 5. Run Verification
        │        └─ PowerShell: .\Verify-Deployment.ps1 -BuildRelease
        │
        ├──► 6. Final Checklist
        │        └─ Review: PRE_DEPLOYMENT_CHECKLIST.md
        │
        └──► 7. Commit & Push
                 └─ git push origin main
                    └─ ✅ READY FOR DEPLOYMENT

```

## 2️⃣ GitHub Actions Workflow

```
┌──────────────────────────────────────────────────────┐
│        GITHUB ACTIONS CI/CD PIPELINE                 │
│   (.github/workflows/deploy-azure.yml)              │
└──────────────────────────────────────────────────────┘

TRIGGER: Push to main/master branch
         ↓

┌──────────────────────────────────────────────────────┐
│  SETUP JOB                                           │
│  ─────────────────────────────────────────────────── │
│  • OS: ubuntu-latest                                 │
│  • .NET Version: 9.x                                │
│  • Continue on error: false                         │
└──────────────────────────────────────────────────────┘
                     ↓

┌──────────────────────────────────────────────────────┐
│  BUILD JOB: "build"                                 │
│  ─────────────────────────────────────────────────── │
│                                                      │
│  Step 1: Checkout Code                             │
│  ├─ Uses: actions/checkout@v4                      │
│  └─ Gets latest code from GitHub                   │
│                                                      │
│  Step 2: Setup .NET 9                              │
│  ├─ Uses: actions/setup-dotnet@v4                  │
│  └─ Installs .NET 9 SDK                            │
│                                                      │
│  Step 3: Restore Dependencies                      │
│  ├─ Command: dotnet restore                        │
│  └─ Restores NuGet packages                        │
│                                                      │
│  Step 4: Build Solution                            │
│  ├─ Command: dotnet build -c Release               │
│  ├─ Builds in Release configuration                │
│  └─ Order: Model → DAL → Admin → Learner           │
│                                                      │
│  Step 5: Run Tests (Optional)                      │
│  ├─ Currently: Skipped                             │
│  └─ Add test projects when ready                   │
│                                                      │
│  Step 6: Publish LMS_admin                         │
│  ├─ Command: dotnet publish -c Release             │
│  ├─ Output: ./publish/LMS_admin/                   │
│  └─ Includes: Code, Config, Dependencies           │
│                                                      │
│  Step 7: Publish LMS_learner                       │
│  ├─ Command: dotnet publish -c Release             │
│  ├─ Output: ./publish/LMS_learner/                 │
│  └─ Includes: Code, Config, Dependencies           │
│                                                      │
│  Step 8: Upload Artifacts                          │
│  ├─ Uses: actions/upload-artifact@v3               │
│  ├─ Preserves published files                      │
│  └─ Retention: 1 day                               │
│                                                      │
│  Result: ✅ SUCCESS or ❌ FAILURE                   │
│                                                      │
└──────────────────────────────────────────────────────┘
                     ↓

┌──────────────────────────────────────────────────────┐
│  DEPLOY JOB: "deploy"                               │
│  ─────────────────────────────────────────────────── │
│  • Runs only if: Push to main/master                │
│  • Depends on: "build" job success                 │
│  • OS: ubuntu-latest                                │
│                                                      │
│  Step 1: Checkout Code                             │
│  └─ Gets fresh copy from GitHub                    │
│                                                      │
│  Step 2: Download Artifacts                        │
│  ├─ Uses: actions/download-artifact@v3             │
│  └─ Gets published apps from build                 │
│                                                      │
│  Step 3: Login to Azure                            │
│  ├─ Uses: azure/login@v1                           │
│  ├─ Credentials: AZURE_CREDENTIALS secret          │
│  └─ Creates authenticated session                  │
│                                                      │
│  Step 4: Deploy to App Service                     │
│  ├─ Uses: azure/webapps-deploy@v2                  │
│  ├─ Target: elg-prod-9                             │
│  ├─ Package: ./publish/                            │
│  └─ Deploys both apps to Azure                     │
│                                                      │
│  Step 5: Health Check                              │
│  ├─ Waits 30 seconds for startup                   │
│  ├─ Tests: https://.../manage/                     │
│  ├─ Tests: https://.../learn/                      │
│  └─ Expected: HTTP 200 or 302                      │
│                                                      │
│  Step 6: Notification                              │
│  ├─ On success: Deployment complete                │
│  └─ Shows: Access URLs                             │
│                                                      │
│  Result: ✅ DEPLOYED or ❌ FAILED                   │
│                                                      │
└──────────────────────────────────────────────────────┘

Total Workflow Time: ~5-10 minutes
```

## 3️⃣ Azure App Service Architecture

```
┌────────────────────────────────────────────────────────┐
│              AZURE APP SERVICE                         │
│              elg-prod-9                                │
│              (UK South Region)                         │
└────────────────────────────────────────────────────────┘

Inside Azure App Service:
│
├──► Port 80 (HTTP)
│    └─► Incoming Request: https://elg-prod-9-cmbadsewffa3brg2.uksouth-01.azurewebsites.net/manage
│         │
│         └──► IIS Module
│              └──► Kestrel (.NET Runtime)
│                   │
│                   ├──► URL Rewrite Rules (web.config)
│                   │    └─► Check if path matches /manage or /learn
│                   │
│                   ├──► If /manage/* 
│                   │    └─► Route to LMS_admin application
│                   │         │
│                   │         └──► LMS_admin Middleware Pipeline
│                   │              ├─ Authentication
│                   │              ├─ Routing
│                   │              ├─ Dependency Injection
│                   │              └─ Controllers/Views
│                   │
│                   └──► If /learn/*
│                        └─► Route to LMS_learner application
│                             │
│                             └──► LMS_learner Middleware Pipeline
│                                  ├─ Authentication (SAML)
│                                  ├─ Routing
│                                  ├─ Dependency Injection
│                                  └─ Controllers/Views
│
├──► Application Settings
│    ├─ ASPNETCORE_ENVIRONMENT = Production
│    ├─ Database Connection String
│    ├─ SMTP Configuration
│    └─ Application Insights Key
│
├──► Connection String
│    └─► Azure SQL Database: new_elg_data
│         │
│         ├──► adminDBEntity.Context
│         ├──► learnerDbEntity.Context
│         └──► Stored Procedures (100+)
│
└──► Monitoring
     ├─ Application Insights
     ├─ Metrics (CPU, Memory, Requests)
     └─ Logs (Application, IIS, Failed Requests)
```

## 4️⃣ Request Flow Example

```
USER REQUESTS: /manage/dashboard
        │
        ▼
HTTPS PROXY (Azure Front Door / Application Gateway)
        │
        ▼ (HTTPS → HTTP converted)
IIS Server (Port 80)
        │
        ▼
web.config URL Rewrite Module
        │
        ├─ Check: Does path start with /manage?
        │  └─ YES ✓
        │
        ▼
Rewrite Rule Activates
        │
        └─ Original Path:  /manage/dashboard
           │
           ▼ (Rewritten transparently)
           
           New Path: /LMS_admin/dashboard
           │
           ▼
Kestrel ASP.NET Core Runtime
        │
        ├─ Load LMS_admin application
        └─► Middleware Pipeline
            │
            ├─► Authentication Middleware
            │   └─ Check user login
            │
            ├─► Routing Middleware
            │   └─ Match: /dashboard route
            │
            ├─► Dependency Injection
            │   └─ Inject: Services, Repositories
            │
            ├─► Controller Execution
            │   └─ DashboardController.Index()
            │
            ├─► Entity Framework Query
            │   │
            │   └─► Azure SQL Database
            │       │
            │       ├─ Stored Procedure: sp_GetDashboardData
            │       ├─ Query: SELECT * FROM AdminDashboard
            │       │
            │       └─► RETURN: Admin Dashboard Data
            │
            ├─► View Rendering
            │   └─ Render: Dashboard.cshtml
            │
            └─► Response Generation
                │
                └─ HTTP 200 OK
                   Content-Type: text/html
                   Body: <html>...</html>
        │
        ▼
Browser Receives Response
        │
        ▼
Renders Dashboard Page
```

## 5️⃣ Rollback Flow

```
┌──────────────────────────────────────────────────────┐
│              DEPLOYMENT ISSUE DETECTED               │
│              (Critical error after deploy)           │
└──────────────────────────────────────────────────────┘
        │
        ▼
OPTION 1: Azure Portal Rollback (Fastest)
        │
        ├─► Go to: App Service → Deployments
        │   │
        │   └─► Select Previous Working Deployment
        │       │
        │       └─► Click: Restart
        │           │
        │           └─► Azure reverts to previous version
        │               │
        │               ▼
        │               ⏱️ Time: < 2 minutes
        │               
        │
        └─► SYSTEM BACK ONLINE ✅


OPTION 2: GitHub Rollback (Recommended)
        │
        ├─► In GitHub:
        │   ├─ Go to: Commits or Pull Requests
        │   │
        │   └─ Find: Last working commit
        │       │
        │       └─► Revert commit
        │           │
        │           └─► Push revert to main branch
        │               │
        │               └─► GitHub Actions Triggered
        │                   │
        │                   ├─ Build previous version
        │                   ├─ Publish previous version
        │                   └─ Deploy to Azure
        │                       │
        │                       ▼
        │                       ⏱️ Time: 5-10 minutes
        │
        └─► SYSTEM BACK ONLINE ✅


OPTION 3: Manual Restoration
        │
        ├─► If database corrupted:
        │   └─ Restore from Azure SQL Backup
        │       │
        │       └─► Point-in-time restore
        │
        └─► ⏱️ Time: 10-30 minutes
```

## 6️⃣ Post-Deployment Monitoring

```
┌─────────────────────────────────────────────────────┐
│      POST-DEPLOYMENT MONITORING                     │
└─────────────────────────────────────────────────────┘
        │
        ├─► HOUR 1: Immediate Checks
        │   ├─ Both URLs responding
        │   ├─ No 5xx errors
        │   ├─ Database connected
        │   └─ Static files loading
        │
        ├─► DAY 1: Functional Testing
        │   ├─ User login works
        │   ├─ Data displays correctly
        │   ├─ File uploads successful
        │   └─ Reports generate
        │
        ├─► WEEK 1: Performance Analysis
        │   ├─ Page load times < 3 seconds
        │   ├─ CPU usage < 50%
        │   ├─ Memory stable
        │   └─ No memory leaks
        │
        └─► ONGOING: Health Checks
            ├─ Application Insights monitoring
            ├─ Alert thresholds configured
            ├─ Weekly log review
            └─ Monthly performance report

Monitoring Sources:
├─ Azure Portal (Metrics)
├─ Application Insights (Exceptions, Requests)
├─ Kudu Dashboard (Process Health)
├─ GitHub Actions (Deployment History)
└─ Azure Monitor (Alerts)
```

## 7️⃣ Deployment Timeline

```
Timeline                 Action                        Duration
─────────────────────────────────────────────────────────────
Now                      Start deployment prep          
  +5 min                 Read documentation             5 min
  +10 min                Update config files            5 min
  +15 min                Run verification script        5 min
  +25 min                Create GitHub secret           10 min
  +30 min                Commit & push code             5 min
  +40 min                GitHub Actions starts build    10 min
  +50 min                Build completes                10 min
  +55 min                Deploy to Azure starts         5 min
  +60 min                Deployment completes           5 min
  +65 min                Health checks pass             5 min
  +70 min                🎉 DEPLOYMENT SUCCESS         ✅

Total Time: ~70 minutes (full process)
  - Preparation: 30 minutes
  - Automated deployment: 40 minutes
```

## Summary

```
┌────────────────────────────────────────────────────┐
│                  DEPLOYMENT FLOW                   │
└────────────────────────────────────────────────────┘

You (Local)
  ↓
GitHub Repository
  ↓
GitHub Actions (Automatic)
  ├─ Build projects
  ├─ Publish applications
  └─ Deploy to Azure
  ↓
Azure App Service (elg-prod-9)
  ├─ IIS + Kestrel
  ├─ URL Rewriting
  ├─ Web.config
  └─ Production Settings
  ↓
Users Access:
  ├─ /manage   → LMS_admin
  └─ /learn    → LMS_learner
  ↓
Azure SQL Database
  └─ new_elg_data
  ↓
✅ SYSTEM OPERATIONAL
```

---

**All components ready. Deployment is straightforward and automated.**
**Next Step: Follow [DEPLOYMENT_QUICK_START.md](DEPLOYMENT_QUICK_START.md)**
