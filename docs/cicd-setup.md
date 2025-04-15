# Setting Up CI/CD for GalaxyWiki API

This document explains how to set up the GitHub Actions workflow to automatically deploy your API to EC2 when pushing to the main branch.

## Overview

The deployment workflow will:

1. Build your .NET 9.0 API project
2. Package it for deployment
3. Send it to your EC2 instance
4. Set up and restart the service

## Required GitHub Secrets

To enable the workflow, you need to add the following secrets to your GitHub repository:

### For Database Connection

- `DB_HOST`: Your PostgreSQL database hostname
- `DB_PORT`: Your PostgreSQL database port (typically 5432)
- `DB_NAME`: Your database name
- `DB_USERNAME`: Your database username
- `DB_PASSWORD`: Your database password

### For EC2 Deployment

- `EC2_HOST`: Your EC2 instance's public IP or DNS
- `EC2_USERNAME`: SSH username (typically `ec2-user`)
- `EC2_SSH_PRIVATE_KEY`: Your EC2 SSH private key (the entire key content)

## Setting Up GitHub Secrets

1. Go to your GitHub repository
2. Click on "Settings"
3. In the left sidebar, click on "Secrets and variables" > "Actions"
4. Click on "New repository secret"
5. Add each secret with its name and value

### Getting Values for Secrets

Most of these values can be obtained from your Terraform outputs:

```bash
cd infra
terraform output database_endpoint  # For DB_HOST
terraform output ec2_host           # For EC2_HOST
terraform output ssh_username       # For EC2_USERNAME
terraform output -raw ssh_private_key > ssh_key.pem  # For EC2_SSH_PRIVATE_KEY
# Copy the entire contents of ssh_key.pem for EC2_SSH_PRIVATE_KEY
```

## Testing the Workflow

After setting up the secrets:

1. Make a change to your API code in the `src` directory
2. Commit and push the change to the `main` branch
3. Go to the "Actions" tab in your GitHub repository to monitor the workflow

The workflow will automatically:
- Build the API
- Deploy it to your EC2 instance
- Set up the systemd service if it doesn't exist
- Restart the service

## Troubleshooting

If the workflow fails, check:

1. GitHub Actions logs for detailed error messages
2. EC2 instance systemd logs:
   ```bash
   ssh ec2-user@<your-ec2-host>
   sudo journalctl -u galaxywiki-api.service -f
   ```
3. Verify all secrets are correctly set in GitHub 