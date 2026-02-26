@echo off
REM Deploy ELG.Web to Azure
cd /d D:\Net-Project\elgLMS_NET9

echo.
echo ===== PUBLISHING APPLICATION =====
echo.

REM Publish to Release folder
dotnet publish ELG.Web/ELG.Web.csproj -c Release -o ./publish --no-build

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Publish failed!
    pause
    exit /b 1
)

echo.
echo ===== DEPLOYING TO AZURE =====
echo.

REM Deploy to Azure using ZIP
echo Creating ZIP package...
powershell -Command "Compress-Archive -Path './publish/*' -DestinationPath './publish.zip' -Force"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: ZIP creation failed!
    pause
    exit /b 1
)

echo.
echo Uploading to Azure App Service...
az webapp deployment source config-zip ^
  --resource-group atf-prod-core-infra-rg ^
  --name elg-prod ^
  --src ./publish.zip

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Azure deployment failed!
    echo Make sure you're logged in to Azure: az login
    pause
    exit /b 1
)

echo.
echo ===== DEPLOYMENT COMPLETE =====
echo.
echo Waiting for app service to restart (2-3 minutes)...
echo.
timeout /t 180

echo.
echo You can now test the upload at: https://app.elearningate.com/CourseManagement/UploadScormPackage
echo.
pause
