using System;
using System.Collections.Generic;
using System.Text;

namespace VideoSpider.Cache
{
    public interface ICacheManager
    {
        string Get(string key);

        T Get<T>(string key);


        bool Set(string key, string data, int cacheTime = 0);

        bool Set<T>(string key, T data, int cacheTime = 0);


        bool IsExist(string key);


        bool Remove(string key);
    }
}
