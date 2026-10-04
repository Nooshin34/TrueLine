$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$settingsPath = Join-Path $root "BackEnd\appsettings.json"
$exe = "C:\minio\minio.exe"
$data = "C:\minio-data"

if (-not (Test-Path $exe)) {
    throw "MinIO was not found at $exe"
}

if (-not (Test-Path $settingsPath)) {
    throw "Create BackEnd\appsettings.json before starting MinIO."
}

$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
$userName = [string]$settings.Minio.AccessKey
$password = [string]$settings.Minio.SecretKey
if ([string]::IsNullOrWhiteSpace($userName) -or [string]::IsNullOrWhiteSpace($password)) {
    throw "Fill Minio AccessKey and SecretKey in BackEnd\appsettings.json and keep them unchanged."
}

$listen = Get-NetTCPConnection -LocalPort 9000 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($listen) {
    Write-Host "MinIO is already running on port 9000."
    exit 0
}

New-Item -ItemType Directory -Force -Path $data | Out-Null
$env:MINIO_ROOT_USER = $userName
$env:MINIO_ROOT_PASSWORD = $password
Write-Host "Starting MinIO from $data. Keep this window open."
Write-Host "The site uses the AccessKey and SecretKey already saved in appsettings.json."
& $exe server $data --address ":9000" --console-address ":9001"
