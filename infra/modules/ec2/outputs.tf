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

output "ssh_private_key" {
  description = "The private key for SSH access (sensitive)"
  value       = tls_private_key.ssh_key.private_key_pem
  sensitive   = true
}

output "ssh_key_name" {
  description = "The name of the generated SSH key"
  value       = aws_key_pair.generated_key.key_name
}

output "ssh_username" {
  description = "The username for SSH access"
  value       = "ec2-user"  # For Amazon Linux AMI
} 