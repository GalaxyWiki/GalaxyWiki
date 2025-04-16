#!/bin/bash

# Change to the infrastructure directory
cd "$(dirname "$0")/../infra"

# Extract the private key from Terraform outputs
terraform output -raw ssh_private_key > galaxy-api-key.pem

# Set the correct permissions for the key file
chmod 600 galaxy-api-key.pem

# Get the username and host for SSH
EC2_USERNAME=$(terraform output -raw ssh_username)
EC2_HOST=$(terraform output -raw ec2_host)

echo "SSH private key saved to: infra/galaxy-api-key.pem"
echo "EC2 Username: $EC2_USERNAME"
echo "EC2 Host: $EC2_HOST"
echo ""
echo "You can SSH to the instance with:"
echo "ssh -i galaxy-api-key.pem $EC2_USERNAME@$EC2_HOST"
echo ""
echo "For GitHub Actions, you need to add the following secrets:"
echo "- EC2_SSH_PRIVATE_KEY: The content of galaxy-api-key.pem"
echo "- EC2_HOST: $EC2_HOST"
echo "- EC2_USERNAME: $EC2_USERNAME" 