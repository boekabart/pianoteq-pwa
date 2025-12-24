#!/usr/bin/env pwsh
# Deployment script for Pianoteq PWA

param(
    [Parameter(Mandatory=$false)]
    [string]$Action = "start",
    [string]$Version = "latest",
    [int]$Port = 8080
)

$ErrorActionPreference = "Stop"

function Show-Help {
    Write-Host "🎹 Pianoteq PWA Deployment" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\deploy.ps1 [action] [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Actions:" -ForegroundColor Cyan
    Write-Host "  start       Start the PWA container (default)" -ForegroundColor Gray
    Write-Host "  stop        Stop the PWA container" -ForegroundColor Gray
    Write-Host "  restart     Restart the PWA container" -ForegroundColor Gray
    Write-Host "  logs        View container logs" -ForegroundColor Gray
    Write-Host "  status      Show container status" -ForegroundColor Gray
    Write-Host "  clean       Stop and remove container" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Cyan
    Write-Host "  -Version    Docker image version (default: latest)" -ForegroundColor Gray
    Write-Host "  -Port       Host port to bind (default: 8080)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\deploy.ps1 start -Port 3000" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 logs" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 restart -Version 1.0.0" -ForegroundColor Gray
}

switch ($Action.ToLower()) {
    "start" {
        Write-Host "🚀 Starting Pianoteq PWA..." -ForegroundColor Cyan
        docker run -d `
            --name pianoteq-pwa `
            -p ${Port}:80 `
            --restart unless-stopped `
            pianoteq-pwa:$Version
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ PWA started successfully!" -ForegroundColor Green
            Write-Host "   Access at: http://localhost:$Port" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "💡 Next steps:" -ForegroundColor Cyan
            Write-Host "   1. Open http://localhost:$Port in your browser" -ForegroundColor Gray
            Write-Host "   2. Click install icon in address bar to add to home screen" -ForegroundColor Gray
            Write-Host "   3. Ensure Pianoteq is running with JSON-RPC enabled" -ForegroundColor Gray
        } else {
            Write-Host "❌ Failed to start container" -ForegroundColor Red
            exit 1
        }
    }
    
    "stop" {
        Write-Host "⏸️  Stopping Pianoteq PWA..." -ForegroundColor Cyan
        docker stop pianoteq-pwa
        Write-Host "✅ Container stopped" -ForegroundColor Green
    }
    
    "restart" {
        Write-Host "🔄 Restarting Pianoteq PWA..." -ForegroundColor Cyan
        docker restart pianoteq-pwa
        Write-Host "✅ Container restarted" -ForegroundColor Green
    }
    
    "logs" {
        Write-Host "📜 Container logs (Ctrl+C to exit):" -ForegroundColor Cyan
        docker logs -f pianoteq-pwa
    }
    
    "status" {
        Write-Host "📊 Container status:" -ForegroundColor Cyan
        docker ps -a --filter name=pianoteq-pwa --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    }
    
    "clean" {
        Write-Host "🧹 Cleaning up..." -ForegroundColor Cyan
        docker stop pianoteq-pwa 2>$null
        docker rm pianoteq-pwa 2>$null
        Write-Host "✅ Container removed" -ForegroundColor Green
    }
    
    "help" {
        Show-Help
    }
    
    default {
        Write-Host "❌ Unknown action: $Action" -ForegroundColor Red
        Write-Host ""
        Show-Help
        exit 1
    }
}
