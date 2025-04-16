resource "aws_vpc" "api_vpc" {
  cidr_block = "10.1.0.0/16"
  enable_dns_support = true
  enable_dns_hostnames = true
  
  tags = {
    Name = "api-vpc"
  }
}

resource "aws_subnet" "api_subnet" {
  vpc_id            = aws_vpc.api_vpc.id
  cidr_block        = "10.1.1.0/24"
  availability_zone = "eu-west-1a"
  map_public_ip_on_launch = true
  
  tags = {
    Name = "api-subnet"
  }
}

resource "aws_internet_gateway" "api_igw" {
  vpc_id = aws_vpc.api_vpc.id
  
  tags = {
    Name = "api-igw"
  }
}

resource "aws_route_table" "api_rt" {
  vpc_id = aws_vpc.api_vpc.id
  
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.api_igw.id
  }
  
  tags = {
    Name = "api-rt"
  }
}

resource "aws_route_table_association" "api_rta" {
  subnet_id      = aws_subnet.api_subnet.id
  route_table_id = aws_route_table.api_rt.id
}

resource "aws_security_group" "api_sg" {
  name        = "galaxy-api-sg"
  description = "Allow web and SSH traffic"
  vpc_id      = aws_vpc.api_vpc.id
  
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "HTTP"
  }
  
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "HTTPS"
  }
  
  ingress {
    from_port   = 5000
    to_port     = 5001
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = ".NET API Ports"
  }
  
  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "SSH"
  }
  
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  tags = {
    Name = "galaxy-api-sg"
  }
}

resource "aws_iam_role" "ec2_role" {
  name = "galaxy-ec2-role"
  
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ssm_policy" {
  role       = aws_iam_role.ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

# Add EC2 Instance Connect policy
resource "aws_iam_role_policy_attachment" "ec2_instance_connect_policy" {
  role       = aws_iam_role.ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/EC2InstanceConnect"
}

# Add S3 access for artifacts
resource "aws_iam_role_policy_attachment" "s3_access_policy" {
  role       = aws_iam_role.ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess"
}

resource "aws_iam_instance_profile" "ec2_profile" {
  name = "galaxy-ec2-profile"
  role = aws_iam_role.ec2_role.name
}

data "aws_ami" "amazon_linux" {
  most_recent = true
  owners      = ["amazon"]
  
  filter {
    name   = "name"
    values = ["amzn2-ami-hvm-*-x86_64-gp2"]
  }
  
  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

# User data script to bootstrap the instance
locals {
  user_data = <<-EOF
    #!/bin/bash
    # Update system
    yum update -y
    
    # Install required packages
    yum install -y git
    
    # Install AWS CLI
    curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
    yum install -y unzip
    unzip awscliv2.zip
    ./aws/install
    aws --version

    # Install .NET 9.0
    rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
    yum install -y dotnet-sdk-9.0
    
    # Create directory for application
    mkdir -p /var/www/galaxywiki
    chown -R ec2-user:ec2-user /var/www/galaxywiki
    
    # Set environment variables for database connection
    cat > /etc/profile.d/galaxy-env.sh << 'ENVFILE'
    export DB_HOST="${var.db_host}"
    export DB_PORT="${var.db_port}"
    export DB_NAME="${var.db_name}"
    export DB_USERNAME="${var.db_username}"
    export DB_PASSWORD="${var.db_password}"
    export ASPNETCORE_URLS="http://0.0.0.0:5000"
    export ASPNETCORE_ENVIRONMENT="Production"
    ENVFILE
    
    # Make environment variables available to services
    source /etc/profile.d/galaxy-env.sh
    
    # Create systemd service for the API
    cat > /etc/systemd/system/galaxywiki-api.service << 'SERVICEFILE'
    [Unit]
    Description=GalaxyWiki API
    After=network.target

    [Service]
    WorkingDirectory=/var/www/galaxywiki
    ExecStart=/usr/bin/dotnet /var/www/galaxywiki/GalaxyWiki.API.dll
    Restart=always
    RestartSec=10
    SyslogIdentifier=galaxywiki-api
    User=ec2-user
    Environment=ASPNETCORE_ENVIRONMENT=Production
    Environment=DOTNET_GCHeapHardLimit=200000000
    Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
    Environment=DB_HOST=${var.db_host}
    Environment=DB_PORT=${var.db_port}
    Environment=DB_NAME=${var.db_name}
    Environment=DB_USERNAME=${var.db_username}
    Environment=DB_PASSWORD=${var.db_password}

    [Install]
    WantedBy=multi-user.target
    SERVICEFILE
    
    # Reload systemd
    systemctl daemon-reload
    
    # Note: The API will be started after deployment
    # You need to deploy your application to /var/www/galaxywiki
  EOF
}

resource "tls_private_key" "ssh_key" {
  algorithm = "RSA"
  rsa_bits  = 4096
}

resource "aws_key_pair" "generated_key" {
  key_name   = "galaxy-api-key"
  public_key = tls_private_key.ssh_key.public_key_openssh
}

resource "aws_instance" "api_server" {
  ami                    = data.aws_ami.amazon_linux.id
  instance_type          = "t2.micro"
  subnet_id              = aws_subnet.api_subnet.id
  vpc_security_group_ids = [aws_security_group.api_sg.id]
  key_name              = aws_key_pair.generated_key.key_name
  iam_instance_profile  = aws_iam_instance_profile.ec2_profile.name
  
  user_data = local.user_data
  user_data_replace_on_change = true

  root_block_device {
    volume_type           = "gp2"
    volume_size           = 8
    delete_on_termination = true
  }
  
  tags = {
    Name = "galaxy-api-server"
    Environment = "Production"
  }
}

resource "aws_eip" "api_eip" {
  instance = aws_instance.api_server.id
  vpc      = true
  
  tags = {
    Name = "galaxy-api-eip"
  }
}
