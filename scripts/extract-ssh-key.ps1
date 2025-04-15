# Script to extract SSH key from Terraform outputs for GitHub Actions

# Navigate to the infrastructure directory
Push-Location -Path ..\infra

# Check if terraform command is available
if (-not (Get-Command terraform -ErrorAction SilentlyContinue)) {
    Write-Error "Terraform is not installed or not in the PATH. Please install Terraform."
    exit 1
}

# Run terraform init if .terraform directory doesn't exist
if (-not (Test-Path -Path .\.terraform)) {
    Write-Host "Initializing Terraform..."
    terraform init
}

# Get the SSH private key from terraform output
Write-Host "Extracting SSH private key..."
terraform output -raw ssh_private_key > galaxy-api-key.pem

# Check if the key was extracted successfully
if (Test-Path -Path .\galaxy-api-key.pem) {
    $keyContent = Get-Content -Path .\galaxy-api-key.pem -Raw
    if ($keyContent) {
        Write-Host "SSH key extracted successfully to galaxy-api-key.pem"
        Write-Host "To add this key to GitHub secrets, copy its contents to the EC2_SSH_PRIVATE_KEY secret."
        Write-Host "Contents are also copied to clipboard for convenience."
        $keyContent | Set-Clipboard
    } else {
        Write-Error "SSH key file is empty. The key might not be generated yet. Run 'terraform apply' first."
    }
} else {
    Write-Error "Failed to extract SSH key. Make sure you've run 'terraform apply' first."
}

# Get other useful outputs
Write-Host "`nOther important values for GitHub secrets:"
Write-Host "EC2_HOST: $(terraform output ec2_host)"
Write-Host "EC2_USERNAME: $(terraform output ssh_username)"
Write-Host "DB_HOST: $(terraform output database_endpoint)"

# Return to the original directory
Pop-Location 