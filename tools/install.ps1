# Tailwind CLI Downloader for Windows
$url = "https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe"
$target = "./tailwindcss.exe"

Write-Host "Downloading Tailwind CLI from $url..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $url -OutFile $target

Write-Host "Done! Binary saved as tools/tailwindcss.exe" -ForegroundColor Green
