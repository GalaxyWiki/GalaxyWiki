#!/bin/bash

# Update system packages
sudo apt-get update
sudo apt-get upgrade -y

# Install .NET dependencies
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-7.0
sudo apt-get install -y aspnetcore-runtime-7.0

# Setup application directory
sudo mkdir -p /var/www/galaxywiki-api
sudo chown -R $USER:$USER /var/www/galaxywiki-api
sudo chmod -R 755 /var/www/galaxywiki-api

# Copy systemd service file
sudo cp galaxywiki-api.service /etc/systemd/system/

# Reload systemd daemon
sudo systemctl daemon-reload

# Enable our service to start at boot
sudo systemctl enable galaxywiki-api.service

echo "EC2 instance setup completed." 