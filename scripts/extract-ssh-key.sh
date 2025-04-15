#!/bin/bash
# Script to extract SSH key from Terraform outputs for GitHub Actions

# Navigate to the infrastructure directory
cd ../infra || { echo "Error: infra directory not found"; exit 1; }

# Check if terraform command is available
if ! command -v terraform &> /dev/null; then
    echo "Error: terraform is not installed or not in the PATH. Please install Terraform."
    exit 1
fi

# Run terraform init if .terraform directory doesn't exist
if [ ! -d ".terraform" ]; then
    echo "Initializing Terraform..."
    terraform init
fi

# Get the SSH private key from terraform output
echo "Extracting SSH private key..."
terraform output -raw ssh_private_key > galaxy-api-key.pem

# Set appropriate permissions for the key file
chmod 600 galaxy-api-key.pem

# Check if the key was extracted successfully
if [ -s "galaxy-api-key.pem" ]; then
    echo "SSH key extracted successfully to galaxy-api-key.pem"
    echo "To add this key to GitHub secrets, copy its contents to the EC2_SSH_PRIVATE_KEY secret."
    echo "You can view the key with: cat galaxy-api-key.pem"
else
    echo "Error: SSH key file is empty or not created. The key might not be generated yet. Run 'terraform apply' first."
    exit 1
fi

# Get other useful outputs
echo -e "\nOther important values for GitHub secrets:"
echo "EC2_HOST: $(terraform output ec2_host)"
echo "EC2_USERNAME: $(terraform output ssh_username)"
echo "DB_HOST: $(terraform output database_endpoint)"

# Return to the original directory
cd - > /dev/null 