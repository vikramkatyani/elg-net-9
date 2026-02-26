@echo off
REM Quick deploy to Azure

cd /d D:\Net-Project\elgLMS_NET9

echo Publishing...
dotnet publish ELG.Web/ELG.Web.csproj -c Release -o ./publish --no-build

echo.
echo Creating ZIP package...
Compress-Archive -Path './publish/*' -DestinationPath './publish.zip' -Force

echo.
echo Deploying to Azure...
az webapp deployment source config-zip --resource-group atf-prod-core-infra-rg --name elg-prod --src ./publish.zip

echo.
echo Deployment complete! Waiting for app to restart...
echo Waiting 180 seconds...
timeout /t 180

echo.
echo Test at: https://app.elearningate.com/CourseManagement/UploadScormPackage
echo.
