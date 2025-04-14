@echo off
powershell -ExecutionPolicy Bypass -File "%~dp0\destroy-ec2-terraform.ps1"
pause 