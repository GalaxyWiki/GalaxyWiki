resource "aws_vpc" "db_vpc" {
  cidr_block = "10.0.0.0/16"
  
  enable_dns_support = true
  enable_dns_hostnames = true
  
  tags = {
    Name = "db-vpc"
  }
}

resource "aws_subnet" "db_subnet_1" {
  vpc_id                  = aws_vpc.db_vpc.id
  cidr_block              = "10.0.1.0/24"
  availability_zone       = "eu-west-1a"
  map_public_ip_on_launch = true
  
  tags = {
    Name = "db-subnet-1"
  }
}

resource "aws_subnet" "db_subnet_2" {
  vpc_id                  = aws_vpc.db_vpc.id
  cidr_block              = "10.0.2.0/24"
  availability_zone       = "eu-west-1b"
  map_public_ip_on_launch = true
  
  tags = {
    Name = "db-subnet-2"
  }
}

resource "aws_db_subnet_group" "db_subnet_group" {
  name       = "galaxy-db-subnet-group"
  subnet_ids = [aws_subnet.db_subnet_1.id, aws_subnet.db_subnet_2.id]
  
  tags = {
    Name = "galaxy-db-subnet-group"
  }
}

resource "aws_security_group" "db_sg" {
  name        = "galaxy-db-sg"
  description = "Allow inbound traffic to the database"
  vpc_id      = aws_vpc.db_vpc.id
  
  ingress {
    from_port   = var.db_port
    to_port     = var.db_port
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]  # Allow connections from anywhere
  }
  
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  tags = {
    Name = "galaxy-db-sg"
  }
}

resource "aws_db_instance" "galaxy_db" {
  identifier             = "galaxy-db"
  allocated_storage      = 20
  storage_type           = "gp2"
  engine                 = "postgres"
  engine_version         = "17.4"
  instance_class         = "db.t4g.micro"
  db_name                = var.db_name
  username               = var.db_username
  password               = var.db_password
  port                   = var.db_port
  parameter_group_name   = "default.postgres17"
  skip_final_snapshot    = true
  db_subnet_group_name   = aws_db_subnet_group.db_subnet_group.name
  vpc_security_group_ids = [aws_security_group.db_sg.id]
  
  multi_az               = false
  publicly_accessible    = true
  
  tags = {
    Name = "galaxy-db"
  }
}

resource "aws_internet_gateway" "db_igw" {
  vpc_id = aws_vpc.db_vpc.id
  
  tags = {
    Name = "db-igw"
  }
}

resource "aws_route_table" "db_rt" {
  vpc_id = aws_vpc.db_vpc.id
  
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.db_igw.id
  }
  
  tags = {
    Name = "db-rt"
  }
}

resource "aws_route_table_association" "db_rta_1" {
  subnet_id      = aws_subnet.db_subnet_1.id
  route_table_id = aws_route_table.db_rt.id
}

resource "aws_route_table_association" "db_rta_2" {
  subnet_id      = aws_subnet.db_subnet_2.id
  route_table_id = aws_route_table.db_rt.id
} 