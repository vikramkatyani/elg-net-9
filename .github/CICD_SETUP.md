# GitHub Actions CI/CD Setup Guide

## Overview
This CI/CD pipeline automates building, testing, and deploying the ELG.NET9 application to Azure App Service.

## Workflow Stages

### 1. **Build Stage** (Runs on all pushes and PRs)
- Restores NuGet packages
- Builds the solution in Release configuration
- Runs tests (if available)
- Publishes the application
- Uploads artifact

### 2. **Deploy Stage** (Runs only on push to main branch)
- Downloads published artifact
- Creates deployment package
- Authenticates with Azure
- Deploys to Azure App Service (elg-prod)
- Cleans up Azure credentials

### 3. **Notify Stage** (Runs on completion)
- Reports build and deployment status

## Required Secrets

Add these secrets to your GitHub repository:

### AZURE_CREDENTIALS
This is a JSON service principal for Azure authentication.

**To create it:**
```powershell
# Login to Azure
az login

# Create a service principal
az ad sp create-for-rbac --name "elg-cicd-sp" --role contributor --scopes /subscriptions/{SUBSCRIPTION_ID}/resourceGroups/atf-prod-core-infra-rg

# The output will be:
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "..."
}

# Convert to proper format for GitHub secret:
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "...",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.microsoft.com/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

## Setup Steps

1. **Add Azure Credentials to GitHub**
   - Go to Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `AZURE_CREDENTIALS`
   - Value: Paste the JSON from above

2. **Configure Branch Protection** (Optional but recommended)
   - Go to Settings → Branches
   - Add protection rule for `main` branch
   - Require status checks to pass before merging
   - Dismiss stale PR approvals when new commits are pushed

3. **Monitor Workflow**
   - Go to Actions tab
   - Click on workflow run to view detailed logs
   - Each stage shows success/failure status

## Triggers

The workflow runs automatically on:
- ✅ Push to `main` or `develop` branches
- ✅ Pull requests to `main` or `develop` branches

Deployment to Azure only happens when:
- Code is pushed directly to `main` branch
- Build stage completes successfully

## Manual Workflow Dispatch

To run the workflow manually without pushing code:
1. Go to Actions tab
2. Select "CI/CD Pipeline"
3. Click "Run workflow"
4. Select branch and click "Run workflow"

## Troubleshooting

### Build Fails
- Check .NET version matches (currently 9.0.x)
- Verify all dependencies in package.json/csproj are restorable
- Check for compilation errors in the build log

### Deployment Fails
- Verify Azure credentials are valid
- Check App Service slot is ready
- Verify deployment package isn't corrupted
- Check Azure resource group and app name are correct

### Azure Login Fails
- Azure credentials secret may be expired
- Regenerate service principal credentials
- Update the secret in GitHub

## Best Practices

1. **Always test locally before pushing**
   ```bash
   dotnet build
   dotnet test
   ```

2. **Use meaningful commit messages**
   - Include what changed and why

3. **Create pull requests for code review**
   - Workflow will run and verify no conflicts with main

4. **Monitor deployment logs**
   - Check GitHub Actions logs for any warnings
   - Check Azure App Service logs for runtime issues

5. **Keep secrets secure**
   - Never commit credentials to repository
   - Rotate service principal credentials periodically
   - Use branch protection to prevent accidental merges

## Pipeline Status Badge

Add this to your README.md:
```markdown
![CI/CD Pipeline](https://github.com/vikramkatyani/elg-net-9/actions/workflows/ci-cd.yml/badge.svg)
```

## Contact & Support

For issues with the CI/CD pipeline, check:
- GitHub Actions logs in the Actions tab
- Azure App Service logs
- Application logs in `/app-logs/` directory
