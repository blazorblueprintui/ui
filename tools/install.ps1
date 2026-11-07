# Tailwind CLI Downloader for Windows

# Pinned for reproducible builds; override with $env:TAILWIND_VERSION = "vX.Y.Z"
$version = if ($env:TAILWIND_VERSION) { $env:TAILWIND_VERSION } else { "v4.3.5" }
$url = "https://github.com/tailwindlabs/tailwindcss/releases/download/$version/tailwindcss-windows-x64.exe"
$target = "./tailwindcss.exe"

Write-Host "Downloading Tailwind CLI from $url..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $url -OutFile $target

Write-Host "Done! Binary saved as tools/tailwindcss.exe" -ForegroundColor Green
