# .NET 9 Migration - Deployment Checklist

**Project:** elgLMS_NET9 Migration  
**Date:** January 1, 2026  
**Status:** Ready for Deployment

---

## Pre-Deployment Checklist

### Environment Verification
- [ ] **Windows Server 2016 or later** - Required for System.Drawing.Common
- [ ] **.NET 9 Runtime installed** - `dotnet --version` shows 9.0.x
- [ ] **IIS 10 or later** - Hosting ASP.NET Core applications
- [ ] **SQL Server 2019 or later** - Database connectivity verified
- [ ] **Sufficient disk space** - At least 2GB free
- [ ] **Sufficient memory** - 4GB+ available for application pools

### Database Preparation
- [ ] **Database backup created** - `new_elg_data` backup taken
- [ ] **Connection strings verified** in appsettings.json:
  ```
  Server: correct SQL Server instance
  Database: new_elg_data
  Authentication: Integrated Security or SQL Auth
  ```
- [ ] **All stored procedures exist** - Verified in database
- [ ] **Entity Framework migrations tested** (if any)
- [ ] **Tables and indexes intact** - No corruption
- [ ] **Backup restoration tested** - Can restore if needed

### Application Configuration
- [ ] **appsettings.json reviewed** - All values correct:
  - [ ] Connection strings (lmsdbEntities, learnerDBEntities)
  - [ ] Email settings (SMTP server, credentials)
  - [ ] SendGrid API key
  - [ ] Azure Storage connection strings
  - [ ] Base URLs (https://elearningate.com/manage/, etc.)
  - [ ] Security keys and encryption keys
  
- [ ] **appsettings.Development.json created** for dev environment
- [ ] **appsettings.Production.json created** (if separate needed)
- [ ] **Environment variables set** (if using external config)
- [ ] **Secrets configured** - Not hardcoded in files

### Security Verification
- [ ] **SSL/TLS certificates installed** - Valid and not expired
- [ ] **HTTPS enforced** - HTTP redirects to HTTPS
- [ ] **CORS configured correctly** if needed
- [ ] **API keys secured** - Not in source control
- [ ] **Database credentials encrypted** - Not in plain text
- [ ] **Session cookies configured** - Secure flag set
- [ ] **CSRF protection enabled** - Token validation active

### Build Verification  
- [ ] **Release build successful** - No errors
  ```powershell
  dotnet build -c Release
  dotnet publish -c Release
  ```
- [ ] **All NuGet packages restored** correctly
- [ ] **Warnings reviewed** - None critical
- [ ] **Published files include appsettings.json** - Config files present
- [ ] **Static files in wwwroot** - CSS, JS, images present

### Application Dependencies
- [ ] **EntityFramework 6.5.1** - Working with .NET 9
- [ ] **System.Configuration.ConfigurationManager** - For config access
- [ ] **Microsoft.AspNetCore.SystemWebAdapters** - For compatibility
- [ ] **log4net 3.0.3** - Logging configured
- [ ] **SendGrid 9.29.3** - Email sending ready
- [ ] **Azure Storage packages** - Blob storage access ready
- [ ] **All other NuGet packages** - Compatible with .NET 9

---

## Deployment Steps

### Phase 1: Pre-Deployment (Day Before)

#### 1. Database Backup
```powershell
# Create backup of current database
BACKUP DATABASE [new_elg_data] 
TO DISK = 'D:\Backups\new_elg_data_$(Get-Date -Format "yyyyMMdd_HHmmss").bak'
GO
```

#### 2. Application Backup
```powershell
# Backup current IIS applications
cd C:\inetpub\wwwroot
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Copy-Item -Path "LMS_admin" -Destination "LMS_admin_backup_$timestamp" -Recurse
Copy-Item -Path "LMS_learner" -Destination "LMS_learner_backup_$timestamp" -Recurse
```

#### 3. Prepare Release Build
```powershell
# Clean previous builds
cd D:\Net-Project\elgLMS_NET9
Remove-Item -Path ".\publish" -Recurse -Force -ErrorAction SilentlyContinue

# Create release builds
dotnet publish LMS_admin\LMS_admin.csproj -c Release -o ".\publish\LMS_admin"
dotnet publish LMS_learner\LMS_learner.csproj -c Release -o ".\publish\LMS_learner"

# Verify output
Get-ChildItem -Path ".\publish\LMS_admin" -Recurse | Measure-Object
Get-ChildItem -Path ".\publish\LMS_learner" -Recurse | Measure-Object
```

#### 4. Configuration Files
```powershell
# Copy and update configuration files
Copy-Item -Path "LMS_admin\appsettings.json" -Destination ".\publish\LMS_admin\"
Copy-Item -Path "LMS_learner\appsettings.json" -Destination ".\publish\LMS_learner\"

# Edit configuration for production values (DO NOT COMMIT TO SOURCE CONTROL)
# Use secure configuration management for sensitive data
```

---

### Phase 2: Deployment (Deployment Day)

#### Step 1: Stop Application Pools (7:00 AM)
```powershell
# Stop IIS application pools
Import-Module WebAdministration

Stop-WebAppPool -Name "LMS_admin"
Stop-WebAppPool -Name "LMS_learner"

# Verify stopped
Get-WebAppPoolState -Name "LMS_admin"
Get-WebAppPoolState -Name "LMS_learner"

# Wait for graceful shutdown (30 seconds)
Start-Sleep -Seconds 30
```

#### Step 2: Deploy LMS_admin (7:05 AM)
```powershell
# Remove old deployment
$adminPath = "C:\inetpub\wwwroot\LMS_admin"
Remove-Item -Path "$adminPath\*" -Recurse -Force -ErrorAction SilentlyContinue

# Copy new deployment
Copy-Item -Path "D:\Net-Project\elgLMS_NET9\publish\LMS_admin\*" `
          -Destination $adminPath -Recurse -Force

# Verify deployment
Get-ChildItem -Path $adminPath | Select-Object Name, FullName | Format-Table
```

#### Step 3: Deploy LMS_learner (7:10 AM)
```powershell
# Remove old deployment
$learnerPath = "C:\inetpub\wwwroot\LMS_learner"
Remove-Item -Path "$learnerPath\*" -Recurse -Force -ErrorAction SilentlyContinue

# Copy new deployment
Copy-Item -Path "D:\Net-Project\elgLMS_NET9\publish\LMS_learner\*" `
          -Destination $learnerPath -Recurse -Force

# Verify deployment
Get-ChildItem -Path $learnerPath | Select-Object Name, FullName | Format-Table
```

#### Step 4: Start Application Pools (7:15 AM)
```powershell
# Start IIS application pools
Start-WebAppPool -Name "LMS_admin"
Start-WebAppPool -Name "LMS_learner"

# Verify started
Get-WebAppPoolState -Name "LMS_admin"
Get-WebAppPoolState -Name "LMS_learner"

# Wait for startup
Start-Sleep -Seconds 15
```

---

### Phase 3: Post-Deployment Verification (7:30 AM)

#### Step 1: Application Accessibility
- [ ] **LMS_admin accessible** - https://yourserver/manage/
  ```powershell
  Invoke-WebRequest -Uri "https://yourserver/manage/" -UseBasicParsing
  ```
- [ ] **LMS_learner accessible** - https://yourserver/
  ```powershell
  Invoke-WebRequest -Uri "https://yourserver/" -UseBasicParsing
  ```
- [ ] **HTTP → HTTPS redirect working**
- [ ] **SSL certificate valid** - No warnings

#### Step 2: Login Functionality
- [ ] **Admin login page loads** - No errors
- [ ] **Learner login page loads** - No errors
- [ ] **Login authentication works** - Test user account
- [ ] **Session management working** - Can navigate pages

#### Step 3: Database Connectivity
- [ ] **Database queries executing** - No timeout errors
- [ ] **Stored procedures working** - Course list loads
- [ ] **Data display correct** - No corrupted records
- [ ] **No N+1 query issues** - Performance acceptable

#### Step 4: File Operations
- [ ] **File upload working** - Can upload document
- [ ] **File download working** - Can download document
- [ ] **Azure Storage integration** - Files accessible
- [ ] **File paths correct** - No 404 errors

#### Step 5: Email Functionality
- [ ] **Email notifications sending** - Check sent/receiving
- [ ] **SMTP connection working** - No timeout errors
- [ ] **Email templates rendering** - Correct formatting
- [ ] **SendGrid integration** - API calls successful

#### Step 6: Application Logging
```powershell
# Check for errors in application logs
Get-EventLog -LogName Application -Source ".NET Runtime" -Newest 20 | 
  Where-Object {$_.EntryType -eq "Error"} | 
  Select-Object TimeGenerated, Message
```

- [ ] **No critical errors** in Event Log
- [ ] **Log4net logging working** - Logs created
- [ ] **Error pages displaying** - No blank screens
- [ ] **Debug information hidden** - No stack traces exposed

#### Step 7: Performance Check
```powershell
# Monitor application pool performance
Get-WebAppPoolState -Name "LMS_admin"
Get-WebAppPoolState -Name "LMS_learner"

# Check CPU and Memory
Get-Process | Where-Object {$_.Name -like "w3wp*"} | 
  Select-Object Name, Handles, CPU, Memory
```

- [ ] **Memory usage stable** - Not continuously growing
- [ ] **CPU usage acceptable** - < 50% baseline
- [ ] **Application pool not recycling** - Stable state
- [ ] **Response times acceptable** - < 2 seconds

---

## Rollback Procedure (If Issues Occur)

### Immediate Rollback
```powershell
# If critical errors detected, rollback immediately:

# 1. Stop application pools
Stop-WebAppPool -Name "LMS_admin"
Stop-WebAppPool -Name "LMS_learner"
Start-Sleep -Seconds 10

# 2. Restore from backup
$timestamp = "20260101_070000"  # Use your backup timestamp
Copy-Item -Path "C:\inetpub\wwwroot\LMS_admin_backup_$timestamp\*" `
          -Destination "C:\inetpub\wwwroot\LMS_admin" -Recurse -Force

Copy-Item -Path "C:\inetpub\wwwroot\LMS_learner_backup_$timestamp\*" `
          -Destination "C:\inetpub\wwwroot\LMS_learner" -Recurse -Force

# 3. Start application pools
Start-WebAppPool -Name "LMS_admin"
Start-WebAppPool -Name "LMS_learner"
Start-Sleep -Seconds 15

# 4. Verify rolled back state
Invoke-WebRequest -Uri "https://yourserver/manage/" -UseBasicParsing
```

### Database Rollback (If Needed)
```powershell
# Only if database was modified
RESTORE DATABASE [new_elg_data]
FROM DISK = 'D:\Backups\new_elg_data_20250101_000000.bak'
WITH REPLACE
GO
```

### Investigation & Resolution
1. Check application logs for specific errors
2. Review event log for system errors
3. Verify database connectivity
4. Check configuration file values
5. Review deployment process for issues
6. Contact application support if needed

---

## Post-Deployment Monitoring (48 Hours)

### Day 1: Active Monitoring
- [ ] **Check application health** - Every 2 hours
- [ ] **Monitor error logs** - No new errors expected
- [ ] **Verify user activities** - Normal usage patterns
- [ ] **Database performance** - No locking or deadlocks
- [ ] **Email delivery** - All notifications sending

### Day 2: Validation
- [ ] **All features tested** - Main workflows executed
- [ ] **Performance baseline** - Compared with pre-migration
- [ ] **User reports** - No issues reported
- [ ] **Security checks** - No suspicious activity
- [ ] **Backup verification** - Can still restore if needed

### Ongoing
- [ ] **Weekly log review** - Check for issues
- [ ] **Monthly performance analysis** - Optimization opportunities
- [ ] **Quarterly security assessment** - Vulnerability checks
- [ ] **Annual architecture review** - Technology updates

---

## Support Contacts

### Escalation Path
1. **Application Support:** Application-support@company.com
2. **Infrastructure Team:** Infrastructure@company.com
3. **Database Team:** Database-team@company.com
4. **On-Call Engineer:** See on-call rotation

### Documentation
- **Deployment Guide:** [Link to this document]
- **Configuration Guide:** See MIGRATION_REPORT.md
- **Troubleshooting:** See technical documentation
- **Change Log:** See Git commit history

---

## Sign-Off

**Deployment Manager:** ___________________  
**Date/Time:** ___________________  

**Application Owner:** ___________________  
**Date/Time:** ___________________  

**Infrastructure Lead:** ___________________  
**Date/Time:** ___________________  

---

**Status:** ✅ APPROVED FOR DEPLOYMENT
