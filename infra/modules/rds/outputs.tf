output "db_endpoint" {
  description = "The endpoint of the database"
  value       = aws_db_instance.galaxy_db.endpoint
}

output "db_id" {
  description = "The ID of the database"
  value       = aws_db_instance.galaxy_db.id
}

output "db_vpc_id" {
  description = "The ID of the VPC"
  value       = aws_vpc.db_vpc.id
}

output "db_security_group_id" {
  description = "The ID of the security group"
  value       = aws_security_group.db_sg.id
} 