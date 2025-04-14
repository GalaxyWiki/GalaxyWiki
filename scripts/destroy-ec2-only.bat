@echo off
powershell -ExecutionPolicy Bypass -File "%~dp0\destroy-ec2-only.ps1"
pause 