provider "aws" {
  region = "eu-west-1"  # Ireland
}

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
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

# Output the endpoints for easy access
output "database_endpoint" {
  description = "The endpoint of the database"
  value       = module.rds.db_endpoint
}

output "api_endpoint" {
  description = "The endpoint of the API"
  value       = module.ec2.api_endpoint
} 