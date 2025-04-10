output "api_endpoint" {
  description = "The endpoint of the API (public IP)"
  value       = aws_eip.api_eip.public_dns
}

output "instance_id" {
  description = "The ID of the EC2 instance"
  value       = aws_instance.api_server.id
}

output "public_ip" {
  description = "The public IP of the EC2 instance"
  value       = aws_eip.api_eip.public_ip
}

output "vpc_id" {
  description = "The ID of the VPC"
  value       = aws_vpc.api_vpc.id
} 