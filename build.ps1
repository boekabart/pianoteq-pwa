#!/usr/bin/env pwsh
# Build script for Pianoteq PWA

param(
    [string]$Version = "0.0.0-local",
    [switch]$SkipTests,
    [switch]$Docker,
    [switch]$Publish
)

$ErrorActionPreference = "Stop"

Write-Host "🎹 Building Pianoteq PWA" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow

if ($Docker) {
    Write-Host "`n📦 Building Docker image..." -ForegroundColor Cyan
    docker build --build-arg VERSION=$Version -t pianoteq-pwa:$Version -t pianoteq-pwa:latest .
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker image built successfully!" -ForegroundColor Green
        Write-Host "   Tagged as: pianoteq-pwa:$Version and pianoteq-pwa:latest" -ForegroundColor Gray
    } else {
        Write-Host "❌ Docker build failed!" -ForegroundColor Red
        exit 1
    }
} elseif ($Publish) {
    Write-Host "`n📦 Publishing release build..." -ForegroundColor Cyan
    dotnet publish src/Pianoteq.Pwa -c Release -p:VERSION=$Version -o ./publish
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Published to ./publish/wwwroot" -ForegroundColor Green
    } else {
        Write-Host "❌ Publish failed!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "`n🔨 Building..." -ForegroundColor Cyan
    dotnet build -c Release -p:VERSION=$Version
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        exit 1
    }
    
    if (-not $SkipTests) {
        Write-Host "`n🧪 Running tests..." -ForegroundColor Cyan
        dotnet test -c Release --no-build
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Tests failed!" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
}

Write-Host "`n📋 Quick commands:" -ForegroundColor Cyan
Write-Host "  Run locally:  cd src/Pianoteq.Pwa && dotnet run" -ForegroundColor Gray
Write-Host "  Run Docker:   docker run -p 8080:80 pianoteq-pwa" -ForegroundColor Gray
Write-Host "  Compose:      docker-compose up -d" -ForegroundColor Gray
