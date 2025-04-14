#!/bin/bash

# Update system packages for Amazon Linux
sudo yum update -y

# Install development tools
sudo yum groupinstall -y "Development Tools"

# Install .NET SDK 9.0
# Reference: https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0.203 --install-dir /usr/share/dotnet
rm dotnet-install.sh

# Create symbolic links
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# Setup application directory
sudo mkdir -p /var/www/galaxywiki-api
sudo chown -R ec2-user:ec2-user /var/www/galaxywiki-api
sudo chmod -R 755 /var/www/galaxywiki-api

# Create systemd service file
cat > galaxywiki-api.service << 'EOF'
[Unit]
Description=GalaxyWiki API Service
After=network.target

[Service]
WorkingDirectory=/var/www/galaxywiki-api
ExecStart=/usr/bin/dotnet /var/www/galaxywiki-api/GalaxyWiki.API.dll
Restart=always
RestartSec=10
SyslogIdentifier=galaxywiki-api
User=ec2-user
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://+:5000

[Install]
WantedBy=multi-user.target
EOF

# Copy systemd service file
sudo mv galaxywiki-api.service /etc/systemd/system/

# Reload systemd daemon
sudo systemctl daemon-reload

# Enable our service to start at boot
sudo systemctl enable galaxywiki-api.service

echo "EC2 instance setup completed." 