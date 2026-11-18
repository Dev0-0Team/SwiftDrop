//Done
using SwiftDrop.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace SwiftDrop.Services.Classes
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public T? Get<T>(string key) 
        {
            if (string.IsNullOrWhiteSpace(key)) return default;

            if (_memoryCache.TryGetValue(key, out T? value))
            {
                return value;
            }
            return default;
        }

        public void Set<T>(string key, T value, TimeSpan expirationTime , TimeSpan slidingTime)
        {
            if (value == null || string.IsNullOrWhiteSpace(key))
                return;


            var cacheOptions = new MemoryCacheEntryOptions
            {
              
                AbsoluteExpirationRelativeToNow = expirationTime,
                SlidingExpiration = slidingTime
            };

            _memoryCache.Set(key, value, cacheOptions);
        }

        public void Remove(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _memoryCache.Remove(key);
        }

        public async Task<(T? Data, bool FromCache)> GetOrCreateAsync<T>(string key, Func<Task<T?>> dbFallback, TimeSpan absoluteTime, TimeSpan slidingTime, bool storeAfterGet = true)
        {

            if (string.IsNullOrWhiteSpace(key)) return (default, false);

            var cachedData = Get<T?>(key);
            if (cachedData != null)
            {
                return (cachedData,true); 
            }

            var freshData = await dbFallback();

            if (freshData != null && storeAfterGet)
            {
                Set(key, freshData, absoluteTime, slidingTime);
            }

            return (freshData,false);
        }

        
    }
}