provider "aws" {
  region = "eu-west-1"  # Ireland
}

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
    tls = {
      source  = "hashicorp/tls"
      version = "~> 4.0"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2.1"
    }
  }
  required_version = ">= 1.0.0"
}

# Database module
module "rds" {
  source = "./modules/rds"
  
  db_name     = var.db_name
  db_username = var.db_username
  db_password = var.db_password
  db_port     = var.db_port
}

# EC2 module for API hosting
module "ec2" {
  source = "./modules/ec2"
  
  db_host     = module.rds.db_endpoint
  db_port     = var.db_port
  db_name     = var.db_name
  db_username = var.db_username
  db_password = var.db_password
}

# Save the private key to a local file
# resource "local_file" "private_key_pem" {
#   content         = module.ec2.ssh_private_key
#   filename        = "${path.module}/galaxy-api-key.pem"
#   file_permission = "0600"  # Proper permissions for private key
# }

# Output the endpoints for easy access
output "database_endpoint" {
  description = "The endpoint of the database"
  value       = module.rds.db_endpoint
}

output "api_endpoint" {
  description = "The endpoint of the API"
  value       = module.ec2.api_endpoint
}

# output "ssh_private_key" {
#   description = "The private key for SSH access (sensitive)"
#   value       = module.ec2.ssh_private_key
#   sensitive   = true
# }

output "ssh_username" {
  description = "The username for SSH access"
  value       = module.ec2.ssh_username
}

output "ec2_host" {
  description = "The public DNS of the EC2 instance"
  value       = module.ec2.api_endpoint
} 