# Pianoteq PWA - Progressive Web App

A modern, installable web application for controlling Pianoteq 9 remotely.

## Features

### 🌐 Progressive Web App
- **Installable**: Add to home screen on mobile devices or install as desktop app
- **Offline-capable**: Service worker caches app assets for offline use
- **Fast loading**: Optimized with caching strategies
- **App-like experience**: Full-screen mode, splash screen, app icon

### 🎵 Preset Management
- **3-tier navigation**: Navigate by favorite/instrument/preset
- **Favorites**: Local storage-based favorites (independent of Pianoteq's favorites)
- **Licensed filter**: Toggle to show only owned presets
- **Visual indicators**: Lock icon for licensed presets, star for favorites
- **Auto-scroll**: Selected preset automatically scrolled into view
- **Quick navigation**: Previous/Next buttons for all navigation modes

### 🎚️ Parameter Control
- **Condition slider**: Adjust piano condition in real-time
- **Live updates**: Changes reflected immediately in Pianoteq
- **More parameters**: Extensible for additional parameter controls

### 🎨 Modern UI
- **Material Design**: Built with MudBlazor components
- **Responsive**: Works on desktop, tablet, and mobile
- **Dark/Light theme**: Follows system preferences
- **Piano-themed**: Custom piano keyboard favicon

## Architecture

### Client-Side
- **Blazor WebAssembly**: .NET 10 running in the browser
- **MudBlazor**: Material Design component library
- **Blazored.LocalStorage**: Client-side favorites persistence
- **Service Worker**: Offline support and caching

### Server-Side
- **nginx**: Static file serving with PWA optimizations
- **Docker**: Containerized deployment
- **Gzip compression**: Reduced bandwidth usage

## Deployment

### Docker Build
The Dockerfile uses multi-stage builds:
1. **Restore**: Fetch NuGet packages
2. **Build**: Compile the application
3. **Test**: Run unit tests (if any)
4. **Publish**: Create optimized release build
5. **Final**: nginx Alpine image serving static files

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS sdk
# ... build stages ...
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=publish /pub/wwwroot /usr/share/nginx/html
```

### nginx Configuration
Special PWA considerations:
- Service worker served with `Cache-Control: max-age=0`
- Manifest file with correct MIME type
- WASM files with proper MIME types
- SPA routing fallback to index.html
- Long-term caching for hashed assets

### Environment Variables
- `VERSION`: Build version (default: 0.0.0-local)
- Server URL configured in `PianoteqClient` (currently hardcoded)

## Development

### Prerequisites
- .NET 10 SDK
- Node.js (for optional tooling)
- Docker (for containerized builds)

### Local Development
```bash
cd src/Pianoteq.Pwa
dotnet watch run
```

The app will hot-reload on file changes.

### Building
```bash
dotnet build
# Output: src/Pianoteq.Pwa/bin/Debug/net10.0/wwwroot
```

### Publishing
```bash
dotnet publish -c Release
# Output: src/Pianoteq.Pwa/bin/Release/net10.0/publish/wwwroot
```

### Docker Build
```bash
# Development build
docker build -t pianoteq-pwa .

# With version
docker build --build-arg VERSION=1.0.0 -t pianoteq-pwa:1.0.0 .

# Run
docker run -p 8080:80 pianoteq-pwa
```

## Service Worker

The app uses two service worker files:

### `service-worker.js` (Development)
- Simple caching strategy
- Used during `dotnet run`
- Manual cache list

### `service-worker.published.js` (Production)
- Uses Blazor's asset manifest
- Automatically includes all framework files
- Optimized for production builds

### Caching Strategy
- **Precache**: index.html, manifest, icons
- **Runtime cache**: Blazor framework files, static assets
- **Network-only**: Pianoteq API calls (no caching for live data)

## Manifest

The `manifest.json` defines the PWA characteristics:

```json
{
  "name": "Pianoteq Remote",
  "short_name": "Pianoteq",
  "display": "standalone",
  "theme_color": "#594AE2",
  "icons": [
    { "src": "icon-192.png", "sizes": "192x192", "purpose": "any maskable" },
    { "src": "icon-512.png", "sizes": "512x512", "purpose": "any maskable" }
  ]
}
```

## Browser Support

### Desktop
- ✅ Chrome 80+
- ✅ Edge 80+
- ✅ Firefox 90+
- ✅ Safari 14+

### Mobile
- ✅ Chrome for Android
- ✅ Safari for iOS 14+
- ✅ Samsung Internet

### PWA Installation
- ✅ Chrome/Edge: Install icon in address bar
- ✅ iOS: Share → Add to Home Screen
- ✅ Android: Install banner / Menu → Install

## Troubleshooting

### Service Worker Not Registering
- Check browser console for errors
- Ensure served over HTTPS (or localhost)
- Clear browser cache and hard reload

### App Not Installing
- Verify manifest.json is accessible
- Check all required icons exist
- Ensure HTTPS (except localhost)
- Some browsers require user interaction

### Favorites Not Persisting
- Check browser's localStorage quota
- Verify no privacy/incognito mode
- Browser must allow localStorage

### Connection Issues
- Verify Pianoteq is running with JSON-RPC enabled
- Check network connectivity to Pianoteq server
- Update server URL in `Program.cs` if needed

## Customization

### Changing Server URL
Edit `src/Pianoteq.Pwa/Program.cs`:
```csharp
builder.Services.AddSingleton(new PianoteqClient("http://YOUR-SERVER:8081"));
```

### Changing Theme Color
Edit `manifest.json` and `index.html`:
```json
"theme_color": "#YOUR-COLOR"
```

### Adding Parameters
Extend `Home.razor` to add more parameter controls following the Condition slider pattern.

### Custom Caching Strategy
Modify `service-worker.published.js` to customize what's cached and when.

## Performance

### Build Size
- WASM runtime: ~2MB (gzipped)
- App DLLs: ~1MB (gzipped)
- MudBlazor: ~500KB (gzipped)
- Total first load: ~3.5MB

### Caching Benefits
- First visit: ~3.5MB download
- Subsequent visits: ~10KB (only index.html revalidation)
- Offline: 0KB (fully cached)

### Lighthouse Score
Target metrics:
- Performance: 90+
- Accessibility: 95+
- Best Practices: 100
- SEO: 100
- PWA: 100

## Security

### Content Security Policy
Currently permissive. For production, add CSP headers in nginx:
```nginx
add_header Content-Security-Policy "default-src 'self'; connect-src 'self' http://192.168.86.66:8081";
```

### CORS
Pianoteq's JSON-RPC server must allow requests from the PWA origin. Configure in Pianoteq settings.

### HTTPS
For production deployment, use HTTPS:
- Required for service worker (except localhost)
- Required for PWA installation
- Protects user data in transit

## License

Same as parent project.
