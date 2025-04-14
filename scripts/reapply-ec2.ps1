# Change to the infrastructure directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -Path "$scriptDir\..\infra"

Write-Host "This script will apply ONLY the EC2 module changes without affecting the RDS database."

# Using arguments array to avoid issues with parsing
$arguments = @(
    "apply",
    "-target=module.ec2"
)

# Execute terraform with the arguments
Write-Host "Running: terraform $($arguments -join ' ')"
& terraform $arguments

# After successful apply, extract the SSH key information
Write-Host ""
Write-Host "Extracting SSH key information..."
& "$scriptDir\extract-ssh-key.ps1" 