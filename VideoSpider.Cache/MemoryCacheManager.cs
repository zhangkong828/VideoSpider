using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace VideoSpider.Cache
{
    public class MemoryCacheManager : ICacheManager
    {
        private static MemoryCache _cache;
        static MemoryCacheManager()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public string Get(string key)
        {
            return Get<string>(key);
        }

        public T Get<T>(string key)
        {
            var val = default(T);
            _cache.TryGetValue<T>(key, out val);
            return val;
        }

        public bool Set(string key, string data, int cacheTime = 0)
        {
            return Set<string>(key, data, cacheTime);
        }

        public bool Set<T>(string key, T data, int cacheTime = 0)
        {
            TimeSpan? expiry = null;
            if (cacheTime > 0)
                expiry = TimeSpan.FromMinutes(cacheTime);

            _cache.Set<T>(key, data, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
            return IsExist(key);
        }

        public bool IsExist(string key)
        {
            return _cache.TryGetValue(key, out _);
        }

        public bool Remove(string key)
        {
            _cache.Remove(key);
            return true;
        }


    }
}
