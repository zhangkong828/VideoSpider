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
        private static IDatabase _db
        {
            get
            {
                return GetConnectionRedisMultiplexer().GetDatabase(0);
            }
        }

        static RedisManager()
        {
            ConnectionString = ConfigurationManager.GetValue("Redis:connectionString");
            DefaultKey = ConfigurationManager.GetValue("Redis:defaultKey");

            _connMultiplexer = ConnectionMultiplexer.Connect(ConnectionString);
        }

        private RedisManager()
        {
        }

        public static IConnectionMultiplexer GetConnectionRedisMultiplexer()
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

        private static string AddKeyPrefix(string key)
        {
            return $"{DefaultKey}:{key}";
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static byte[] Serialize(object obj)
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
        private static T Deserialize<T>(byte[] data)
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
        public static bool StringSet(string key, string value, TimeSpan? expiry = null)
        {
            key = AddKeyPrefix(key);
            return _db.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 保存多个 Key-value
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public static bool StringSet(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
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
        public static string StringGet(string key)
        {
            key = AddKeyPrefix(key);
            return _db.StringGet(key);
        }

        /// <summary>
        /// 存储一个对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static bool StringSet<T>(string key, T value, TimeSpan? expiry = null)
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
        public static T StringGet<T>(string key)
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
        public static double StringIncrement(string key, double value = 1)
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
        public static double StringDecrement(string key, double value = 1)
        {
            key = AddKeyPrefix(key);
            return _db.StringDecrement(key, value);
        }

        #endregion String 操作


        public static bool KeyExists(string key)
        {
            key = AddKeyPrefix(key);
            return _db.KeyExists(key);
        }

        public static bool KeyDelete(string key)
        {
            key = AddKeyPrefix(key);
            return _db.KeyDelete(key);
        }

    }
}
