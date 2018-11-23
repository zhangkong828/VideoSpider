using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using VideoSpider.Infrastructure;

namespace VideoSpider.Cache
{
    public class RedisManager
    {

        private static readonly object Locker = new object();
        private static readonly string DefaultKey;
        private static readonly string ConnectionString;

        private static IConnectionMultiplexer _connMultiplexer;
        private readonly IDatabase _db;

        static RedisManager()
        {
            ConnectionString = ConfigurationManager.GetValue("RedisConnectionString");
            DefaultKey = ConfigurationManager.GetValue("Redis.DefaultKey");

            _connMultiplexer = ConnectionMultiplexer.Connect(ConnectionString);
        }

        public RedisManager(int db = 0)
        {
            _db = _connMultiplexer.GetDatabase(db);
        }

        public IConnectionMultiplexer GetConnectionRedisMultiplexer()
        {
            if (_connMultiplexer == null || !_connMultiplexer.IsConnected)
                lock (Locker)
                {
                    if (_connMultiplexer == null || !_connMultiplexer.IsConnected)
                        _connMultiplexer = ConnectionMultiplexer.Connect(ConnectionString);
                }

            return _connMultiplexer;
        }

        #region 私有方法

        private string AddKeyPrefix(string key)
        {
            return $"{DefaultKey}:{key}";
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                var data = memoryStream.ToArray();
                return data;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private T Deserialize<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(data))
            {
                var result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        #endregion


        #region String 操作

        /// <summary>
        /// 设置 key 并保存字符串（如果 key 已存在，则覆盖值）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan? expiry = null)
        {
            key = AddKeyPrefix(key);
            return _db.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 保存多个 Key-value
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public bool StringSet(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var pairs = keyValuePairs.Select(x => new KeyValuePair<RedisKey, RedisValue>(AddKeyPrefix(x.Key), x.Value));
            return _db.StringSet(pairs.ToArray());
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public string StringGet(string redisKey)
        {
            redisKey = AddKeyPrefix(redisKey);
            return _db.StringGet(redisKey);
        }

        /// <summary>
        /// 存储一个对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet<T>(string key, T value, TimeSpan? expiry = null)
        {
            key = AddKeyPrefix(key);
            var json = Serialize(value);

            return _db.StringSet(key, json, expiry);
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key)
        {
            key = AddKeyPrefix(key);
            return Deserialize<T>(_db.StringGet(key));
        }

        /// <summary>
        /// 在指定 key 处实现增量的递增，如果该键不存在，则在执行前将其设置为 0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double StringIncrement(string key, double value = 1)
        {
            key = AddKeyPrefix(key);
            return _db.StringIncrement(key, value);
        }

        /// <summary>
        /// 在指定 key 处实现增量的递减，如果该键不存在，则在执行前将其设置为 0
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double StringDecrement(string key, double value = 1)
        {
            key = AddKeyPrefix(key);
            return _db.StringDecrement(key, value);
        }

        #endregion String 操作


    }
}
