// Pianoteq PWA Service Worker
const CACHE_NAME = 'pianoteq-pwa-v1';
const RUNTIME_CACHE = 'pianoteq-runtime-v1';

// Files to cache on install
const PRECACHE_URLS = [
  '/',
  '/index.html',
  '/manifest.json',
  '/favicon.svg',
  '/icon-192.png',
  '/icon-512.png'
];

// Install event - precache static assets
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(PRECACHE_URLS))
      .then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
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

// Fetch event - network first for API calls, cache first for static assets
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Don't cache API calls to Pianoteq
  if (url.hostname === 'retropie' || url.port === '8081') {
    // Network only for Pianoteq API
    return;
  }

  // For same-origin requests
  if (url.origin === location.origin) {
    // Blazor framework files and app files - cache first, network fallback
    if (request.destination === 'script' || 
        request.destination === 'style' ||
        request.destination === 'font' ||
        request.url.includes('_framework/') ||
        request.url.includes('.wasm') ||
        request.url.includes('.dll')) {
      event.respondWith(
        caches.match(request).then(cachedResponse => {
          if (cachedResponse) {
            return cachedResponse;
          }
          return caches.open(RUNTIME_CACHE).then(cache => {
            return fetch(request).then(response => {
              // Only cache successful responses
              if (response.ok) {
                cache.put(request, response.clone());
              }
              return response;
            });
          });
        })
      );
      return;
    }

    // For HTML and other resources - network first, cache fallback
    event.respondWith(
      fetch(request)
        .then(response => {
          // Clone and cache successful responses
          if (response.ok) {
            const responseToCache = response.clone();
            caches.open(RUNTIME_CACHE).then(cache => {
              cache.put(request, responseToCache);
            });
          }
          return response;
        })
        .catch(() => {
          // Fallback to cache
          return caches.match(request).then(cachedResponse => {
            return cachedResponse || caches.match('/index.html');
          });
        })
    );
  }
});

// Handle messages from clients
self.addEventListener('message', event => {
  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }
});
