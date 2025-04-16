# GitHub Actions Configuration

This directory contains GitHub Actions workflows for automating deployment tasks.

## Workflows

### 1. Database Migrations (`db.yml`)

Automatically runs Flyway database migrations when changes are pushed to the main branch.

### 2. API Deployment (`deploy-api.yml`)

Automatically builds and deploys the .NET API to EC2 when changes to source code are pushed to the main branch.

## Required Secrets

You need to add the following secrets to your GitHub repository:

### For Database Migrations

- `DB_HOST`: Your PostgreSQL database hostname
- `DB_PORT`: Your PostgreSQL database port
- `DB_NAME`: Your database name
- `DB_USERNAME`: Your database username
- `DB_PASSWORD`: Your database password
- `DB_URL`: Complete JDBC URL for your database

### For API Deployment

- `EC2_HOST`: Your EC2 instance's public IP or DNS
- `EC2_USERNAME`: SSH username (typically `ec2-user`)
- `EC2_SSH_PRIVATE_KEY`: Your EC2 SSH private key (the entire key content)

## How to Add Secrets

1. Go to your GitHub repository
2. Click on "Settings"
3. In the left sidebar, click on "Secrets and variables" > "Actions"
4. Click on "New repository secret"
5. Add each secret with its name and value

For the EC2_SSH_PRIVATE_KEY, you'll need the private key from your Terraform output. You can get it with:

```bash
cd infra
terraform output -raw ssh_private_key > ssh_key.pem
# Copy the contents of ssh_key.pem to the GitHub secret
```

Make sure to follow proper security practices for handling these keys and credentials.
