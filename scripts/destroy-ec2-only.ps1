# Change to the infrastructure directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -Path "$scriptDir\..\infra"

Write-Host "This script will destroy ONLY the EC2 resources while keeping the RDS database intact."
Write-Host "Press Ctrl+C now if you want to cancel."
Start-Sleep -Seconds 5

# List of EC2 resources to target for destruction
$ec2Resources = @(
    "module.ec2.aws_eip.api_eip",
    "module.ec2.aws_instance.api_server",
    "module.ec2.aws_key_pair.generated_key",
    "module.ec2.tls_private_key.ssh_key"
)

# Destroy each EC2 resource individually
foreach ($resource in $ec2Resources) {
    Write-Host "Destroying $resource..."
    # Note the use of single quotes to prevent variable expansion in the command
    $cmd = "terraform destroy -target='$resource' -auto-approve"
    Invoke-Expression $cmd
}

# Destroy supporting infrastructure
$supportingResources = @(
    "module.ec2.aws_iam_instance_profile.ec2_profile",
    "module.ec2.aws_iam_role_policy_attachment.ssm_policy",
    "module.ec2.aws_iam_role.ec2_role",
    "module.ec2.aws_route_table_association.api_rta",
    "module.ec2.aws_route_table.api_rt",
    "module.ec2.aws_internet_gateway.api_igw",
    "module.ec2.aws_security_group.api_sg",
    "module.ec2.aws_subnet.api_subnet",
    "module.ec2.aws_vpc.api_vpc"
)

foreach ($resource in $supportingResources) {
    Write-Host "Destroying $resource..."
    $cmd = "terraform destroy -target='$resource' -auto-approve"
    Invoke-Expression $cmd
}

Write-Host "EC2 resources have been destroyed. RDS database resources should remain intact."
Write-Host "You can now run 'terraform apply' to recreate the EC2 resources with the updated configuration." 