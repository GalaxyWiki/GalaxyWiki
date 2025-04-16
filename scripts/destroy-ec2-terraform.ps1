# Change to the infrastructure directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -Path "$scriptDir\..\infra"

Write-Host "This script will destroy ONLY the EC2 module while keeping the RDS database intact."
Write-Host "Press Ctrl+C now if you want to cancel."
Start-Sleep -Seconds 5

# Using arguments array to avoid issues with parsing
$arguments = @(
    "destroy",
    "-target=module.ec2",
    "-auto-approve"
)

# Execute terraform with the arguments
Write-Host "Running: terraform $($arguments -join ' ')"
& terraform $arguments

Write-Host "EC2 module has been destroyed. RDS database resources should remain intact."
Write-Host "You can now run 'terraform apply' to recreate the EC2 resources with the updated configuration." 