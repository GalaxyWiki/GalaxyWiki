# Deploying GalaxyWiki API to EC2

This document explains how to set up Continuous Integration and Continuous Deployment (CI/CD) for the GalaxyWiki API to an EC2 instance.

## Prerequisites

1. An AWS account with EC2 instance already created
2. GitHub repository with the GalaxyWiki codebase
3. SSH access to your EC2 instance

## Step 1: Configure EC2 Instance

1. Connect to your EC2 instance via SSH
2. Clone this repository or copy the `scripts` directory to your EC2 instance
3. Make the setup script executable and run it:

```bash
chmod +x scripts/setup-ec2.sh
cd scripts
./setup-ec2.sh
```

4. Copy the service file to the systemd directory:

```bash
sudo cp scripts/galaxywiki-api.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable galaxywiki-api.service
```

## Step 2: Set up SSH Keys and GitHub Secrets

### On Windows:

1. Apply the Terraform changes to generate the SSH key:

```powershell
cd infra
terraform init   # To install the required providers
terraform apply
```

2. Extract the SSH credentials using the provided script:

```powershell
# Option 1: Run the batch file
..\scripts\extract-ssh-key.bat

# Option 2: Run the PowerShell script directly
powershell -ExecutionPolicy Bypass -File ..\scripts\extract-ssh-key.ps1
```

3. Copy the SSH private key content to use in GitHub secrets:

```powershell
Get-Content .\galaxy-api-key.pem | Set-Clipboard
```

### On Linux/macOS

1. Apply the Terraform changes to generate the SSH key:

```bash
cd infra
terraform init   # To install the required providers
terraform apply
```

2. Extract the SSH credentials using the provided script:

```bash
chmod +x ../scripts/extract-ssh-key.sh
../scripts/extract-ssh-key.sh
```

3. View the SSH private key content to use in GitHub secrets:

```bash
cat galaxy-api-key.pem
```

### Adding GitHub Secrets

Add the following secrets to your GitHub repository:

1. `AWS_ACCESS_KEY_ID`: Your AWS access key ID
2. `AWS_SECRET_ACCESS_KEY`: Your AWS secret access key
3. `AWS_REGION`: The AWS region where your EC2 instance is running (e.g., `eu-west-1`)
4. `EC2_SSH_PRIVATE_KEY`: The SSH private key content (entire contents of the .pem file)
5. `EC2_HOST`: The public IP or DNS of your EC2 instance
6. `EC2_USERNAME`: The username for SSH access (likely "ec2-user")

## Step 3: Push to Main Branch

Once the GitHub Actions workflow is set up, any push to the `main` branch that includes changes to the API will trigger an automatic deployment to your EC2 instance.

## Troubleshooting

1. Check GitHub Actions run logs for any deployment errors
2. On the EC2 instance, check the service status:

```bash
sudo systemctl status galaxywiki-api.service
```

3. View logs for debugging:

```bash
sudo journalctl -u galaxywiki-api.service -f
```

## Security Considerations

- Restrict SSH access to your EC2 instance
- Use IAM roles with minimal permissions necessary
- Consider using AWS Secrets Manager for sensitive information
- Encrypt all traffic with HTTPS (consider setting up a reverse proxy with Nginx and Let's Encrypt)
