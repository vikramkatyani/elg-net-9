#!/usr/bin/env pwsh
<#
.SYNOPSIS
    .NET 9 Migration Verification Script
    
.DESCRIPTION
    Verifies that all migrated projects build successfully and are ready for deployment
    
.EXAMPLE
    .\Verify-Migration.ps1
    
.NOTES
    Run this script to validate the migration before deployment
#>

param(
    [string]$WorkspacePath = "d:\Net-Project\elgLMS_NET9",
    [switch]$SkipClean,
    [switch]$Verbose
)

# Initialize variables
$ErrorCount = 0
$WarningCount = 0
$SuccessCount = 0

# Colored output functions
function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
    $script:SuccessCount++
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
    $script:ErrorCount++
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
    $script:WarningCount++
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Write-Section {
    param([string]$Title)
    Write-Host "`n" + ("="*60)
    Write-Host $Title
    Write-Host ("="*60)
}

# Main script
Write-Section ".NET 9 Migration Verification"

# Check working directory
Write-Info "Workspace: $WorkspacePath"
if (-not (Test-Path $WorkspacePath)) {
    Write-Error "Workspace path not found: $WorkspacePath"
    exit 1
}

cd $WorkspacePath

# Section 1: Environment Verification
Write-Section "1. ENVIRONMENT VERIFICATION"

# Check .NET version
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Success ".NET SDK installed: $dotnetVersion"
    if ($dotnetVersion -like "9.*") {
        Write-Success ".NET 9 detected"
    } else {
        Write-Warning ".NET version is $dotnetVersion (expected 9.x.x)"
    }
} else {
    Write-Error ".NET SDK not found"
}

# Check Git
if (Get-Command git -ErrorAction SilentlyContinue) {
    Write-Success "Git available"
} else {
    Write-Warning "Git not found in PATH"
}

# Check Visual Studio Build Tools
$msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
if ($msbuild) {
    Write-Success "MSBuild available"
} else {
    Write-Info "MSBuild not in PATH (using dotnet CLI instead)"
}

# Section 2: Project File Verification
Write-Section "2. PROJECT FILE VERIFICATION"

$projects = @(
    "LMS_Model\LMS_Model.csproj",
    "LMS_DAL\LMS_DAL.csproj",
    "LMS_learner\LMS_learner.csproj",
    "LMS_admin\LMS_admin.csproj"
)

$projectStatus = @{}

foreach ($projectFile in $projects) {
    if (Test-Path $projectFile) {
        $projectName = Split-Path (Split-Path $projectFile) -Leaf
        $content = Get-Content $projectFile -Raw
        
        # Check for net9.0 target framework
        if ($content -match "<TargetFramework>net9\.0</TargetFramework>") {
            Write-Success "$projectName targets .NET 9.0"
            $projectStatus[$projectName] = "OK"
        } else {
            Write-Error "$projectName does not target .NET 9.0"
            $projectStatus[$projectName] = "ERROR"
        }
        
        # Check for SDK-style project
        if ($content -match "Sdk=") {
            Write-Success "$projectName uses SDK-style format"
        } else {
            Write-Warning "$projectName does not use SDK-style format"
        }
    } else {
        Write-Error "Project file not found: $projectFile"
        $projectStatus[(Split-Path (Split-Path $projectFile) -Leaf)] = "NOT_FOUND"
    }
}

# Section 3: Clean Previous Builds (Optional)
Write-Section "3. CLEANING PREVIOUS BUILDS"

if ($SkipClean) {
    Write-Info "Skipping clean (--SkipClean flag used)"
} else {
    Write-Info "Removing previous build artifacts..."
    $cleanPaths = @("bin", "obj", "publish")
    $cleanPaths | ForEach-Object {
        Get-ChildItem -Path $_ -Recurse -ErrorAction SilentlyContinue | 
            Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
    }
    Write-Success "Previous builds cleaned"
}

# Section 4: NuGet Restore
Write-Section "4. NUGET PACKAGE RESTORE"

Write-Info "Restoring NuGet packages..."
$restoreOutput = dotnet restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Success "NuGet packages restored successfully"
} else {
    Write-Error "NuGet restore failed"
    if ($Verbose) { Write-Host $restoreOutput }
}

# Section 5: Build Verification
Write-Section "5. BUILD VERIFICATION"

$buildResults = @{}

foreach ($project in @("LMS_Model", "LMS_DAL", "LMS_learner", "LMS_admin")) {
    $projectFile = "$project\$project.csproj"
    Write-Info "Building $project..."
    
    $buildOutput = dotnet build $projectFile -c Release --no-restore 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "$project built successfully"
        $buildResults[$project] = "SUCCESS"
        
        # Check if DLL exists
        $dllPath = "$project\bin\Release\net9.0\$project.dll"
        if (Test-Path $dllPath) {
            $fileSize = (Get-Item $dllPath).Length / 1MB
            Write-Info "  Output: $project.dll ($([Math]::Round($fileSize, 2)) MB)"
        }
    } else {
        Write-Error "$project build failed"
        $buildResults[$project] = "FAILED"
        if ($Verbose) { Write-Host $buildOutput }
    }
}

# Section 6: Dependency Verification
Write-Section "6. DEPENDENCY VERIFICATION"

$criticalDeps = @(
    "EntityFramework/6.5.1",
    "System.Configuration.ConfigurationManager/10.0.1",
    "Microsoft.AspNetCore.SystemWebAdapters/2.2.1",
    "log4net/3.0.3"
)

Write-Info "Checking critical dependencies..."
foreach ($dep in $criticalDeps) {
    $depName = $dep.Split('/')[0]
    # Try to find in packages
    if (Get-ChildItem -Path "packages" -Filter "$($dep.Split('/')[0])*" -Directory -ErrorAction SilentlyContinue) {
        Write-Success "  $dep present"
    } else {
        Write-Warning "  $dep may not be installed (check NuGet cache)"
    }
}

# Section 7: Configuration Verification
Write-Section "7. CONFIGURATION VERIFICATION"

$configFiles = @(
    "LMS_admin\appsettings.json",
    "LMS_learner\appsettings.json"
)

foreach ($configFile in $configFiles) {
    if (Test-Path $configFile) {
        Write-Success "$configFile found"
        
        # Validate JSON
        try {
            $config = Get-Content $configFile | ConvertFrom-Json
            Write-Success "  JSON format valid"
            
            # Check for critical settings
            if ($config.ConnectionStrings.lmsdbEntities) {
                Write-Success "  Connection string configured"
            } else {
                Write-Warning "  Connection string not found"
            }
        } catch {
            Write-Error "  Invalid JSON format: $_"
        }
    } else {
        Write-Error "$configFile not found"
    }
}

# Section 8: Documentation Verification
Write-Section "8. DOCUMENTATION VERIFICATION"

$docs = @(
    "MIGRATION_REPORT.md",
    "DEPLOYMENT_CHECKLIST.md",
    "TECHNICAL_SUMMARY.md",
    "QUICK_REFERENCE.md"
)

foreach ($doc in $docs) {
    if (Test-Path $doc) {
        $size = (Get-Item $doc).Length / 1KB
        Write-Success "$doc present ($([Math]::Round($size, 0)) KB)"
    } else {
        Write-Warning "$doc not found"
    }
}

# Section 9: Summary
Write-Section "VERIFICATION SUMMARY"

Write-Host "`nBuild Results:"
$buildResults.GetEnumerator() | ForEach-Object {
    if ($_.Value -eq "SUCCESS") {
        Write-Success "  $($_.Key): $($_.Value)"
    } else {
        Write-Error "  $($_.Key): $($_.Value)"
    }
}

Write-Host "`nProject Status:"
$projectStatus.GetEnumerator() | ForEach-Object {
    if ($_.Value -eq "OK") {
        Write-Success "  $($_.Key): $($_.Value)"
    } else {
        Write-Error "  $($_.Key): $($_.Value)"
    }
}

# Final Summary
Write-Host "`n" + ("="*60)
Write-Host "FINAL RESULTS"
Write-Host ("="*60)
Write-Host "✅ Success: $SuccessCount"
Write-Host "❌ Errors:  $ErrorCount"
Write-Host "⚠️  Warnings: $WarningCount"
Write-Host ""

if ($ErrorCount -eq 0) {
    Write-Success "MIGRATION VERIFICATION PASSED ✓"
    Write-Host "`nStatus: READY FOR DEPLOYMENT"
    exit 0
} else {
    Write-Error "MIGRATION VERIFICATION FAILED ✗"
    Write-Host "`nPlease fix the errors above before deployment"
    exit 1
}
