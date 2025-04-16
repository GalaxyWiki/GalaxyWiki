# Change to the infrastructure directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -Path "$scriptDir\..\infra"

# Extract the private key from Terraform outputs
terraform output -raw ssh_private_key | Out-File -FilePath "galaxy-api-key.pem" -Encoding ascii

# Get the username and host for SSH
$EC2_USERNAME = terraform output -raw ssh_username
$EC2_HOST = terraform output -raw ec2_host

Write-Host "SSH private key saved to: infra\galaxy-api-key.pem"
Write-Host "EC2 Username: $EC2_USERNAME"
Write-Host "EC2 Host: $EC2_HOST"
Write-Host ""
Write-Host "You can SSH to the instance with:"
Write-Host "ssh -i galaxy-api-key.pem $EC2_USERNAME@$EC2_HOST"
Write-Host ""
Write-Host "For GitHub Actions, you need to add the following secrets:"
Write-Host "- EC2_SSH_PRIVATE_KEY: The content of galaxy-api-key.pem"
Write-Host "- EC2_HOST: $EC2_HOST"
Write-Host "- EC2_USERNAME: $EC2_USERNAME"

# Instructions for viewing and copying key content
Write-Host ""
Write-Host "To view and copy the private key content for GitHub secrets:"
Write-Host "Get-Content .\galaxy-api-key.pem | Set-Clipboard"
Write-Host "This will copy the key content to your clipboard" 