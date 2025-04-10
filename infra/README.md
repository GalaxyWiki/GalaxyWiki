# Galaxy Wiki Infrastructure

This directory contains Terraform configurations for deploying the GalaxyWiki
application on AWS, using free tier services where possible.

## Architecture

- **Database**: PostgreSQL on RDS (t4.micro instance)
- **API**: EC2 instance (t2.micro)
- **Networking**: Separate VPCs for DB and API with proper security groups

## Prerequisites

- AWS CLI installed and configured with appropriate credentials
- Terraform installed (v1.0.0 or later)

## Configuration

The infrastructure uses environment variables for sensitive information:

- `TF_VAR_db_password`: Database password
- `TF_VAR_db_username`: Database username
- `TF_VAR_db_name`: Database name
- `TF_VAR_db_port`: Database port (default: 5432)

These can be set in the `.env` file or exported directly in your shell.

## Deployment

1. Initialise Terraform:

   ```powershell
   terraform init
   ```

2. Plan the deployment:

   ```powershell
   terraform plan
   ```

3. Apply the configuration:

   ```powershell
   terraform apply
   ```

4. After deployment, Terraform will output:
   - Database endpoint
   - API endpoint (EC2 instance's public DNS)
   - API public IP

## Connecting to the EC2 Instance

The EC2 instance can be accessed via SSH:

```powershell
ssh ec2-user@<public_ip>
```

Alternatively, you can use the AWS Systems Manager Session Manager if you have the AWS CLI configured.

## Cleanup

To destroy all resources:

```powershell
terraform destroy
```
