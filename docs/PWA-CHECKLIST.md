# PWA Deployment Checklist

Use this checklist to verify your Pianoteq PWA is properly configured.

## ✅ Pre-Deployment

- [ ] Pianoteq 9 is installed and running
- [ ] JSON-RPC is enabled in Pianoteq (Preferences → Remote control)
- [ ] Server URL in `src/Pianoteq.Pwa/Program.cs` is correct
- [ ] .NET 10 SDK is installed
- [ ] Docker is installed (for containerized deployment)

## ✅ Build Verification

- [ ] `dotnet build` completes without errors
- [ ] Service worker files are generated:
  - `wwwroot/service-worker.js`
  - `wwwroot/service-worker-assets.js`
- [ ] Manifest file exists: `wwwroot/manifest.json`
- [ ] Icons are present:
  - `wwwroot/icon-192.png`
  - `wwwroot/icon-512.png`
  - `wwwroot/favicon.svg`

## ✅ Docker Build

- [ ] `docker build -t pianoteq-pwa .` completes successfully
- [ ] Image contains all files: `docker run --rm pianoteq-pwa ls /usr/share/nginx/html/`
- [ ] Service worker is present in image
- [ ] Manifest is present in image
- [ ] nginx.conf is properly configured for PWA

## ✅ Runtime Testing

### Local Development
- [ ] App runs: `cd src/Pianoteq.Pwa && dotnet run`
- [ ] Browser opens at http://localhost:5000
- [ ] No console errors in browser DevTools
- [ ] Service worker registers (check Application tab in DevTools)

### Docker Deployment
- [ ] Container starts: `docker run -d -p 8080:80 pianoteq-pwa`
- [ ] App accessible at http://localhost:8080
- [ ] nginx logs show no errors: `docker logs pianoteq-pwa`

## ✅ PWA Features

### Service Worker
- [ ] Open DevTools → Application → Service Workers
- [ ] Status shows "activated and running"
- [ ] No registration errors in console

### Manifest
- [ ] Open DevTools → Application → Manifest
- [ ] All fields populated correctly
- [ ] Icons load without errors
- [ ] No manifest warnings

### Installation
- [ ] Install prompt appears (desktop)
- [ ] Install icon shows in address bar
- [ ] App installs successfully
- [ ] Installed app opens in standalone mode
- [ ] App icon shows correctly

### Offline Capability
- [ ] Load the app while online
- [ ] Open DevTools → Network → Check "Offline"
- [ ] Reload the page
- [ ] App loads from cache (no network requests)
- [ ] UI remains functional (except live Pianoteq data)

### Caching
- [ ] Open DevTools → Application → Cache Storage
- [ ] Multiple caches present (precache, runtime)
- [ ] Framework files are cached
- [ ] Static assets are cached

## ✅ Functionality Testing

### Connection
- [ ] App connects to Pianoteq on startup
- [ ] Current preset loads and displays
- [ ] No CORS errors in console

### Navigation
- [ ] Favorite navigation works (prev/next)
- [ ] Instrument navigation works
- [ ] Preset navigation works
- [ ] All navigation modes load correct presets in Pianoteq

### Presets
- [ ] All presets list loads
- [ ] Licensed filter works
- [ ] Clicking preset loads it in Pianoteq
- [ ] Auto-scroll centers selected preset
- [ ] Lock icons show for licensed presets

### Favorites
- [ ] Can add preset to favorites (star icon)
- [ ] Can remove from favorites
- [ ] Favorites persist after page reload
- [ ] Favorites list updates in real-time

### Parameters
- [ ] Condition slider loads current value
- [ ] Dragging slider updates Pianoteq
- [ ] Value changes reflect in UI

## ✅ Browser Compatibility

Test in multiple browsers:

### Desktop
- [ ] Chrome/Edge (Chromium)
- [ ] Firefox
- [ ] Safari (macOS)

### Mobile
- [ ] Chrome for Android
- [ ] Safari for iOS
- [ ] Samsung Internet

### PWA Installation
- [ ] Chrome/Edge: Install from address bar
- [ ] iOS Safari: Share → Add to Home Screen
- [ ] Android Chrome: Install prompt/menu

## ✅ Performance

### Lighthouse Audit
Run Lighthouse in Chrome DevTools:

- [ ] Performance score: 90+
- [ ] Accessibility score: 95+
- [ ] Best Practices score: 100
- [ ] SEO score: 90+
- [ ] PWA score: 100

### Load Times
- [ ] First visit: < 5 seconds
- [ ] Subsequent visits: < 1 second
- [ ] Offline load: < 0.5 seconds

### Network
- [ ] Check DevTools → Network tab
- [ ] First load: ~3-4 MB
- [ ] Subsequent loads: < 100 KB
- [ ] Brotli/Gzip compression active

## ✅ Production Readiness

### Security
- [ ] Served over HTTPS (required for PWA, except localhost)
- [ ] CORS configured on Pianoteq server
- [ ] No security warnings in browser

### Monitoring
- [ ] Container health check working
- [ ] nginx access logs available
- [ ] nginx error logs available

### Documentation
- [ ] README updated with deployment instructions
- [ ] Environment variables documented
- [ ] Known issues documented

## 🐛 Troubleshooting

If any items fail, check:

### Service Worker Issues
- Must be served over HTTPS or localhost
- Clear browser cache and hard reload (Ctrl+Shift+R)
- Check for JavaScript errors in console

### Installation Issues
- Verify manifest.json is accessible
- Ensure all icon files exist
- Check HTTPS requirement
- Try incognito/private mode

### Connection Issues
- Verify Pianoteq is running
- Check JSON-RPC port (default 8081)
- Test API endpoint: `curl http://192.168.86.30:8081`
- Check browser console for CORS errors

### Docker Issues
- Check container logs: `docker logs pianoteq-pwa`
- Verify nginx config: `docker exec pianoteq-pwa nginx -t`
- Test inside container: `docker exec pianoteq-pwa wget -O- localhost`

## 📝 Notes

- Service worker updates check every 60 seconds (configurable in index.html)
- Favorites stored in browser localStorage (not synced across devices)
- PWA works offline but requires Pianoteq connection for live features
- Clear site data in DevTools to reset favorites and cache
