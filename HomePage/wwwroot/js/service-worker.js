const CACHE_NAME = 'my-cache';

self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(CACHE_NAME).then(function (cache) {
            return cache.addAll([
                '/Index',
                '/site.css',
                '/site.js',
                '/AllFoods',
            ]);
        })
    );
});

self.addEventListener('activate', (event) => {
    const cacheWhitelist = [CACHE_NAME];
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (!cacheWhitelist.includes(cacheName)) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});


self.addEventListener('fetch', (event) => {
    event.respondWith(
        // Try to fetch the resource from the network
        fetch(event.request)
            .then((response) => {
                // If the network response is successful, cache it
                if (event.request.method === 'GET' && response.status === 200) {
                    caches.open(CACHE_NAME).then((cache) => {
                        cache.put(event.request, response.clone());
                    });
                }
                return response;
            })
            .catch(() => {
                // If offline, try serving from the cache
                return caches.match(event.request)
                    .then((cachedResponse) => {
                        // If no cached response, show an offline page or fallback
                        return cachedResponse || caches.match('/offline.html');
                    });
            })
    );
});