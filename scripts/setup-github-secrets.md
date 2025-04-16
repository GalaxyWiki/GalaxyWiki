# GitHub Secrets Setup Guide for GalaxyWiki API Deployment

This guide explains the required GitHub Secrets for the API deployment workflow.

## Required Secrets

Set up the following secrets in your GitHub repository:

### AWS and EC2 Credentials
- `AWS_ACCESS_KEY_ID`: Your AWS access key ID
- `AWS_SECRET_ACCESS_KEY`: Your AWS secret access key
- `AWS_REGION`: AWS region where your EC2 and RDS instances are running (e.g., `us-east-1`)
- `EC2_SSH_PRIVATE_KEY`: Your private SSH key to connect to the EC2 instance
- `EC2_HOST`: Public IP or DNS of your EC2 instance
- `EC2_USERNAME`: SSH username for your EC2 instance (usually `ec2-user`)

### Database Connection
- `DB_HOST`: Your RDS PostgreSQL endpoint
- `DB_PORT`: PostgreSQL port (usually `5432`)
- `DB_NAME`: Database name
- `DB_USERNAME`: Database username
- `DB_PASSWORD`: Database password

### SSL and Domain Configuration
- `API_DOMAIN_NAME`: Your domain name for the API (e.g., `api.galaxywiki.com`)
- `CERTBOT_EMAIL`: Email address for Let's Encrypt SSL certificate notifications

## How to Add GitHub Secrets

1. Go to your GitHub repository
2. Click on "Settings" > "Secrets and variables" > "Actions"
3. Click "New repository secret"
4. Add each secret with its name and value
5. Click "Add secret"

## Verifying Your Configuration

After adding all secrets, manually trigger the workflow to verify the deployment works correctly.

## Additional Notes

- The EC2 instance must have the necessary permissions to access the RDS database
- The EC2 security group should allow inbound traffic on ports 80, 443, and 22 (SSH)
- The RDS security group should allow inbound traffic from the EC2 instance on port 5432 