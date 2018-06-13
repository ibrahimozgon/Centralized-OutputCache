using System;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace OutputCacheExample.Web.CacheManagers
{
    public class RedisCacheManager : IDisposable
    {
        private static string _redisChannelName;
        private const int DefaultDb = 10;
        private readonly ISubscriber _subscriber;
        private readonly ConnectionMultiplexer _muxer;

        private static readonly Lazy<RedisCacheManager> Lazy =
            new Lazy<RedisCacheManager>(() => new RedisCacheManager());

        public static RedisCacheManager Instance => Lazy.Value;

        private RedisCacheManager()
        {
            var connection = new ConfigurationOptions
            {
                DefaultDatabase = DefaultDb,
                AbortOnConnectFail = true,
                ConnectRetry = 50,
                SyncTimeout = int.MaxValue,
                EndPoints =
                {
                    "localhost:6379"
                },
                AllowAdmin = true
            };
            _redisChannelName = "__keyspace@" + DefaultDb + "__:*";
            _muxer = ConnectionMultiplexer.Connect(connection);
            _subscriber = _muxer.GetSubscriber();
            Subscribe();
        }


        public T Get<T>(string key)
        {
            var data = ApplicationCacheManager.Instance.Get<T>(key);
            if (data != null)
                return data;

            var stringValue = _muxer.GetDatabase(DefaultDb).StringGet(key);
            if (!stringValue.HasValue)
                return default(T);
            try
            {
                data = Deserialize<T>(stringValue);
                ApplicationCacheManager.Instance.Set(key, data);
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Remove(key);
            }
            if (data != null)
                ApplicationCacheManager.Instance.Set(key, data);
            return data;
        }

        public void Set<T>(string key, T data, int cacheTime)
        {
            var cache = _muxer.GetDatabase(DefaultDb);
            if (data == null)
                return;
            var entryBytes = Serialize(data);
            var expiresIn = TimeSpan.FromMinutes(cacheTime);
            cache.StringSet(key, entryBytes, expiresIn, When.Always, CommandFlags.FireAndForget);

            ApplicationCacheManager.Instance.Set(key, data);
        }

        public void Remove(string key)
        {
            ApplicationCacheManager.Instance.Remove(key);
            _muxer.GetDatabase(DefaultDb).KeyDelete(key);
        }

        public bool IsSet(string key)
        {
            return ApplicationCacheManager.Instance.IsSet(key) || _muxer.GetDatabase(DefaultDb).KeyExists(key);
        }
        public void Dispose()
        {
            _subscriber.Unsubscribe(_redisChannelName);
            _subscriber?.UnsubscribeAll();
            _muxer?.Dispose();
        }

        private void Subscribe()
        {
            //you only have to do this once, then your callback will be invoked.
            _subscriber.Subscribe(_redisChannelName, (channel, notificationType) =>
            {
                // IS YOUR CALLBACK NOT GETTING CALLED???? 
                // -> See comments above about enabling keyspace notifications on your redis instance
                switch (notificationType) // use "Kxge" keyspace notification options to enable all of the below...
                {
                    case "expired": // requires the "Kx" keyspace notification options to be enabled
                    case "rename_from": // requires the "Kg" keyspace notification option to be enabled
                    case "rename_to": // requires the "Kg" keyspace notification option to be enabled
                    case "del": // requires the "Kg" keyspace notification option to be enabled
                    case "evicted": // requires the "Ke" keyspace notification option to be enabled
                    case "set": // requires the "K$" keyspace notification option to be enabled for STRING operations
                        var key = GetPublishKey(channel);
                        ApplicationCacheManager.Instance.Remove(key);
                        break;
                }
            });
        }

        private static string GetPublishKey(string channel)
        {
            var index = channel.IndexOf(':');
            if (index >= 0 && index < channel.Length - 1)
                return channel.Substring(index + 1);

            //we didn't find the delimeter, so just return the whole thing
            return channel;
        }

        #region Utilities

        protected virtual byte[] Serialize(object item)
        {
            var jsonString = JsonConvert.SerializeObject(item, _settings);
            return Encoding.UTF8.GetBytes(jsonString);
        }
        protected virtual T Deserialize<T>(byte[] serializedObject)
        {
            if (serializedObject == null)
                return default(T);

            var jsonString = Encoding.UTF8.GetString(serializedObject);

            return JsonConvert.DeserializeObject<T>(jsonString, _settings);
        }

        // ReSharper disable once InconsistentNaming
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        #endregion
    }
}