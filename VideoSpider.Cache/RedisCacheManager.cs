using System;
using System.Collections.Generic;
using System.Text;

namespace VideoSpider.Cache
{
    public class RedisCacheManager : ICacheManager
    {
        public string Get(string key)
        {
            return RedisManager.StringGet(key);
        }

        public T Get<T>(string key)
        {
            return RedisManager.StringGet<T>(key);
        }

        public bool Set(string key, string data, int cacheTime = 0)
        {
            TimeSpan? expiry = null;
            if (cacheTime > 0)
                expiry = TimeSpan.FromMinutes(cacheTime);
            return RedisManager.StringSet(key, data, expiry);
        }

        public bool Set<T>(string key, T data, int cacheTime = 0)
        {
            TimeSpan? expiry = null;
            if (cacheTime > 0)
                expiry = TimeSpan.FromMinutes(cacheTime);
            return RedisManager.StringSet<T>(key, data, expiry);
        }

        public bool IsExist(string key)
        {
            return RedisManager.KeyExists(key);
        }

        public bool Remove(string key)
        {
            return RedisManager.KeyDelete(key);
        }


    }
}
