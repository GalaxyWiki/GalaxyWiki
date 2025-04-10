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
    yum install -y httpd git
    
    # Start and enable the web server
    systemctl start httpd
    systemctl enable httpd
    
    # Set environment variables for database connection
    cat > /etc/profile.d/galaxy-env.sh << 'ENVFILE'
    export DB_HOST="${var.db_host}"
    export DB_PORT="${var.db_port}"
    export DB_NAME="${var.db_name}"
    export DB_USERNAME="${var.db_username}"
    export DB_PASSWORD="${var.db_password}"
    ENVFILE
    
    # Create a simple index page
    echo "<html><body><h1>GalaxyWiki API</h1><p>Test</p></body></html>" > /var/www/html/index.html
    
    # Set proper permissions
    chmod 644 /var/www/html/index.html
  EOF
}

resource "aws_instance" "api_server" {
  ami                    = data.aws_ami.amazon_linux.id
  instance_type          = "t2.micro"
  subnet_id              = aws_subnet.api_subnet.id
  vpc_security_group_ids = [aws_security_group.api_sg.id]
  iam_instance_profile   = aws_iam_instance_profile.ec2_profile.name
  user_data              = local.user_data
  
  root_block_device {
    volume_type           = "gp2"
    volume_size           = 8
    delete_on_termination = true
  }
  
  tags = {
    Name = "galaxy-api-server"
  }
}

resource "aws_eip" "api_eip" {
  instance = aws_instance.api_server.id
  vpc      = true
  
  tags = {
    Name = "galaxy-api-eip"
  }
}
