# Pre-Deployment Verification Script for Azure Deployment
# Run this script to validate everything is ready for Azure deployment

param(
    [switch]$RunFullTests,
    [switch]$BuildRelease,
    [switch]$PublishApps,
    [string]$Configuration = "Debug"
)

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       .NET 9 LMS Azure Deployment Verification             ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Colors for output
$successColor = "Green"
$warningColor = "Yellow"
$errorColor = "Red"
$infoColor = "Cyan"

# Counters
$passCount = 0
$failCount = 0

# Function to test condition and report
function Test-Condition {
    param(
        [string]$TestName,
        [bool]$Condition,
        [string]$FailureMessage = ""
    )
    
    if ($Condition) {
        Write-Host "✓ $TestName" -ForegroundColor $successColor
        $script:passCount++
    }
    else {
        Write-Host "✗ $TestName" -ForegroundColor $errorColor
        if ($FailureMessage) {
            Write-Host "  └─ $FailureMessage" -ForegroundColor $warningColor
        }
        $script:failCount++
    }
}

# ============================================================================
# PHASE 1: Environment Check
# ============================================================================
Write-Host "`n[PHASE 1] Environment Verification" -ForegroundColor $infoColor
Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor $infoColor

# Check .NET version
try {
    $dotnetVersion = dotnet --version
    $version = $dotnetVersion -split '\.' | Select-Object -First 1
    Test-Condition ".NET 9 SDK installed" ($version -ge 9) "Current: $dotnetVersion"
}
catch {
    Test-Condition ".NET 9 SDK installed" $false "dotnet CLI not found"
}

# Check required tools
Test-Condition "Git installed" (Get-Command git -ErrorAction SilentlyContinue)
Test-Condition "Azure CLI installed" (Get-Command az -ErrorAction SilentlyContinue)

# Check workspace
$workspaceRoot = "d:\Net-Project\elgLMS_NET9"
Test-Condition "Workspace exists" (Test-Path $workspaceRoot) $workspaceRoot

# ============================================================================
# PHASE 2: Project Files Check
# ============================================================================
Write-Host "`n[PHASE 2] Project Structure Verification" -ForegroundColor $infoColor
Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor $infoColor

$projectPath = @{
    "LMS_Model" = "$workspaceRoot\LMS_Model\LMS_Model.csproj"
    "LMS_DAL" = "$workspaceRoot\LMS_DAL\LMS_DAL.csproj"
    "LMS_admin" = "$workspaceRoot\LMS_admin\LMS_admin.csproj"
    "LMS_learner" = "$workspaceRoot\LMS_learner\LMS_learner.csproj"
}

foreach ($project in $projectPath.GetEnumerator()) {
    $exists = Test-Path $project.Value
    Test-Condition "Project: $($project.Name)" $exists
}

# Check configuration files
$configFiles = @{
    "Root web.config" = "$workspaceRoot\web.config"
    "Admin web.config" = "$workspaceRoot\LMS_admin\web.config"
    "Learner web.config" = "$workspaceRoot\LMS_learner\web.config"
    "Admin appsettings" = "$workspaceRoot\LMS_admin\appsettings.json"
    "Learner appsettings" = "$workspaceRoot\LMS_learner\appsettings.json"
    "GitHub Actions workflow" = "$workspaceRoot\.github\workflows\deploy-azure.yml"
}

foreach ($file in $configFiles.GetEnumerator()) {
    $exists = Test-Path $file.Value
    Test-Condition "File: $($file.Name)" $exists
}

# ============================================================================
# PHASE 3: Build Verification
# ============================================================================
Write-Host "`n[PHASE 3] Build Verification" -ForegroundColor $infoColor
Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor $infoColor

if ($BuildRelease) {
    Write-Host "Building projects in Release configuration..." -ForegroundColor $infoColor
    
    try {
        Push-Location $workspaceRoot
        $buildOutput = dotnet build KCLMS48.sln -c Release 2>&1
        $buildSuccess = $LASTEXITCODE -eq 0
        Pop-Location
        
        Test-Condition "Solution builds successfully" $buildSuccess
        
        if ($buildSuccess) {
            Write-Host "Build completed successfully" -ForegroundColor $successColor
        }
        else {
            Write-Host "Build errors detected:" -ForegroundColor $errorColor
            $buildOutput | Select-String "error" | ForEach-Object { Write-Host "  $_" -ForegroundColor $errorColor }
        }
    }
    catch {
        Test-Condition "Solution builds successfully" $false $_
    }
}
else {
    Write-Host "Skipping Release build (use -BuildRelease flag to enable)" -ForegroundColor $warningColor
}

# ============================================================================
# PHASE 4: Configuration Validation
# ============================================================================
Write-Host "`n[PHASE 4] Configuration Validation" -ForegroundColor $infoColor
Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor $infoColor

# Check web.config rewrite rules
$rootWebConfig = "$workspaceRoot\web.config"
if (Test-Path $rootWebConfig) {
    [xml]$webConfig = Get-Content $rootWebConfig
    $hasManageRule = $webConfig.SelectSingleNode("//rule[@name='Rewrite manage to LMS_admin']") -ne $null
    $hasLearnRule = $webConfig.SelectSingleNode("//rule[@name='Rewrite learn to LMS_learner']") -ne $null
    
    Test-Condition "URL rewrite rule for /manage" $hasManageRule
    Test-Condition "URL rewrite rule for /learn" $hasLearnRule
}

# Check GitHub Actions workflow
$workflowFile = "$workspaceRoot\.github\workflows\deploy-azure.yml"
if (Test-Path $workflowFile) {
    $workflowContent = Get-Content $workflowFile -Raw
    Test-Condition "GitHub Actions workflow exists" $true
    
    # Check critical settings
    $hasAzureLogin = $workflowContent -match "azure/login"
    $hasWebAppsDeploy = $workflowContent -match "azure/webapps-deploy"
    $hasBuild = $workflowContent -match "dotnet build"
    $hasPublish = $workflowContent -match "dotnet publish"
    
    Test-Condition "Workflow has Azure login" $hasAzureLogin
    Test-Condition "Workflow has webapp deploy step" $hasWebAppsDeploy
    Test-Condition "Workflow has build step" $hasBuild
    Test-Condition "Workflow has publish step" $hasPublish
}

# ============================================================================
# PHASE 5: Git Repository Check
# ============================================================================
Write-Host "`n[PHASE 5] Git Repository Status" -ForegroundColor $infoColor
Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor $infoColor

try {
    Push-Location $workspaceRoot
    $gitStatus = git status --porcelain
    $gitLog = git log --oneline -1
    Pop-Location
    
    if ($gitStatus) {
        Write-Host "⚠ Uncommitted changes detected:" -ForegroundColor $warningColor
        $gitStatus | ForEach-Object { Write-Host "  $_" }
    }
    else {
        Write-Host "✓ No uncommitted changes" -ForegroundColor $successColor
        $passCount++
    }
    
    Write-Host "✓ Latest commit: $gitLog" -ForegroundColor $successColor
    $passCount++
}
catch {
    Write-Host "⚠ Git check skipped" -ForegroundColor $warningColor
}

# ============================================================================
# PHASE 6: Sensitive Data Check
# ============================================================================
Write-Host "`n[PHASE 6] Security Checks" -ForegroundColor $infoColor
Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor $infoColor

# Check for hardcoded secrets
$sensitivePatterns = @(
    'password\s*[=:]\s*[''"]'
    'api[_-]?key\s*[=:]\s*[''"]'
    'secret\s*[=:]\s*[''"]'
    'token\s*[=:]\s*[''"]'
)

$projectFiles = Get-ChildItem $workspaceRoot -Include "*.cs", "*.json" -Recurse -ErrorAction SilentlyContinue | 
    Where-Object { $_.FullName -notmatch '(bin|obj|packages)' }

$secretsFound = 0
foreach ($file in $projectFiles) {
    $content = Get-Content $file -Raw
    foreach ($pattern in $sensitivePatterns) {
        if ($content -match $pattern) {
            Write-Host "⚠ Potential secret found in: $($file.Name)" -ForegroundColor $warningColor
            $secretsFound++
        }
    }
}

if ($secretsFound -eq 0) {
    Write-Host "✓ No hardcoded secrets detected" -ForegroundColor $successColor
    $passCount++
}
else {
    Write-Host "✗ Found $secretsFound potential secrets" -ForegroundColor $errorColor
    $failCount++
}

# ============================================================================
# PHASE 7: Publish and Deployment
# ============================================================================
if ($PublishApps) {
    Write-Host "`n[PHASE 7] Publishing Applications" -ForegroundColor $infoColor
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor $infoColor
    
    try {
        Push-Location $workspaceRoot
        
        # Publish LMS_admin
        Write-Host "Publishing LMS_admin..." -ForegroundColor $infoColor
        $adminPublish = dotnet publish LMS_admin/LMS_admin.csproj -c Release -o ./publish/LMS_admin 2>&1
        $adminSuccess = $LASTEXITCODE -eq 0
        Test-Condition "Publish LMS_admin" $adminSuccess
        
        # Publish LMS_learner
        Write-Host "Publishing LMS_learner..." -ForegroundColor $infoColor
        $learnerPublish = dotnet publish LMS_learner/LMS_learner.csproj -c Release -o ./publish/LMS_learner 2>&1
        $learnerSuccess = $LASTEXITCODE -eq 0
        Test-Condition "Publish LMS_learner" $learnerSuccess
        
        Pop-Location
        
        if ($adminSuccess -and $learnerSuccess) {
            Write-Host "`nPublished applications ready in ./publish/" -ForegroundColor $successColor
        }
    }
    catch {
        Write-Host "Publishing failed: $_" -ForegroundColor $errorColor
        $failCount++
    }
}

# ============================================================================
# SUMMARY
# ============================================================================
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                    VERIFICATION SUMMARY                     ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$totalTests = $passCount + $failCount
$percentPass = if ($totalTests -gt 0) { [math]::Round(($passCount / $totalTests) * 100, 1) } else { 0 }

Write-Host "`nTests Passed: $passCount/$totalTests ($percentPass%)" -ForegroundColor $successColor
if ($failCount -gt 0) {
    Write-Host "Tests Failed: $failCount/$totalTests" -ForegroundColor $errorColor
}

Write-Host "`n" 

if ($failCount -eq 0) {
    Write-Host "✓ All checks passed! Ready for deployment." -ForegroundColor $successColor
    Write-Host "`nNext Steps:" -ForegroundColor $infoColor
    Write-Host "  1. Verify Azure credentials are configured (AZURE_CREDENTIALS secret)"
    Write-Host "  2. Update appsettings.Production.json with actual database connection string"
    Write-Host "  3. Commit changes: git add . && git commit -m 'Prepare for Azure deployment'"
    Write-Host "  4. Push to main branch: git push origin main"
    Write-Host "  5. Monitor GitHub Actions: https://github.com/vikramkatyani/elgLMS_NET9/actions"
    Write-Host ""
}
else {
    Write-Host "✗ Some checks failed. Please review the errors above." -ForegroundColor $errorColor
    Write-Host "`nFailing Items:" -ForegroundColor $errorColor
    Write-Host "  • Review the red ✗ marks above"
    Write-Host "  • Fix issues before deployment"
    Write-Host "  • Re-run this script to verify: .\Verify-Deployment.ps1"
    Write-Host ""
}

Write-Host "Verification complete!" -ForegroundColor $infoColor
Write-Host "For more details, see: AZURE_DEPLOYMENT_GUIDE.md" -ForegroundColor $infoColor
