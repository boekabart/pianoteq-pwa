// This is the production version of the service worker
// Blazor will use this in published builds

// Import Blazor's default service worker assets
self.importScripts('./service-worker-assets.js');

const CACHE_NAME = 'pianoteq-pwa-v1';
const RUNTIME_CACHE = 'pianoteq-runtime-v1';

// Get Blazor's assets from service-worker-assets.js
const assetsManifest = self.assetsManifest;
const assetsToCache = assetsManifest.assets.map(asset => new Request(asset.url, { cache: 'no-cache' }));

// Add our custom URLs
const customCache = [
  '/',
  '/manifest.json',
  '/favicon.svg',
  '/icon-192.png',
  '/icon-512.png'
];

// Install event
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        // Cache Blazor assets
        return cache.addAll(assetsToCache);
      })
      .then(() => caches.open(RUNTIME_CACHE))
      .then(cache => {
        // Cache custom assets
        return cache.addAll(customCache);
      })
      .then(() => self.skipWaiting())
  );
});

// Activate event
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames
          .filter(cacheName => cacheName !== CACHE_NAME && cacheName !== RUNTIME_CACHE)
          .map(cacheName => caches.delete(cacheName))
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch event
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Don't cache API calls to Pianoteq
  if (url.hostname === '192.168.86.30' || url.port === '8081') {
    return;
  }

  // For same-origin requests
  if (url.origin === location.origin) {
    event.respondWith(
      caches.match(request).then(cachedResponse => {
        if (cachedResponse) {
          return cachedResponse;
        }
        return fetch(request).then(response => {
          if (response.ok) {
            const responseToCache = response.clone();
            caches.open(RUNTIME_CACHE).then(cache => {
              cache.put(request, responseToCache);
            });
          }
          return response;
        }).catch(() => {
          return caches.match('/index.html');
        });
      })
    );
  }
});

// Handle messages
self.addEventListener('message', event => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
});
