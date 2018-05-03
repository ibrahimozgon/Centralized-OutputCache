using System;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace OutputCacheExample.CacheManagers
{
    public class RedisCacheManager
    {
        private const string RedisChannelName = "outputCache";
        private const int DefaultDb = 10;
        private readonly ConnectionMultiplexer _redis;
        private IDatabase Database => _redis.GetDatabase();
        private ISubscriber Subscriber => _redis.GetSubscriber();
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
            _redis = ConnectionMultiplexer.Connect(connection);
        }

        public ArabamCacheModel Get(string key)
        {
            var rValue = Database.StringGet(key);
            if (!rValue.HasValue)
                return null;
            try
            {
                var result = Deserialize(rValue);
                return result;
            }
            catch (Exception e)
            {
                Console.Write(e);
                Remove(key);
            }
            return null;
        }

        public void Set(ArabamCacheModel data, bool publish = true)
        {
            if (data == null)
                return;
            var entryBytes = Serialize(data);
            var expiresIn = TimeSpan.FromMinutes(data.Timeout);

            Database.StringSetAsync(data.Key, entryBytes, expiresIn, When.Always, CommandFlags.FireAndForget)
                .ConfigureAwait(false);
            if (publish)
                Publish(data);
        }

        public bool IsSet(string key)
        {
            return Database.KeyExists(key);
        }

        public void Remove(string key, bool publish = true)
        {
            Database.KeyDeleteAsync(key).ConfigureAwait(false);
            if (publish)
            {
                Publish(new ArabamCacheModel
                {
                    Key = key
                });
            }
        }

        public void RemoveByPattern(string pattern, bool publish = true)
        {
            foreach (var endPoint in _redis.GetEndPoints())
            {
                var keys = _redis.GetServer(endPoint).Keys(DefaultDb, pattern, int.MaxValue);
                foreach (var key in keys)
                {
                    Database.KeyDeleteAsync(key).ConfigureAwait(false);
                    Publish(new ArabamCacheModel
                    {
                        Key = key
                    });
                }
            }
        }

        public void Clear()
        {
            var eps = _redis.GetEndPoints();
            foreach (var endPoint in eps)
            {
                var server = _redis.GetServer(endPoint);
                //foreach (var key in server.Keys(DefaultDb))
                //{
                //    Remove(key);
                //}

                server.FlushDatabase(DefaultDb);
            }
        }

        public void Subscribe(Action<ArabamCacheModel> consumer)
        {
            Subscriber
                .SubscribeAsync(new RedisChannel(RedisChannelName, RedisChannel.PatternMode.Literal), (channel, message) => consumer(Deserialize(message)))
                .ConfigureAwait(false);
        }

        public void UnSubcribe()
        {
            Subscriber
                .UnsubscribeAllAsync()
                .ConfigureAwait(false);
        }

        public void Publish(ArabamCacheModel data)
        {
            Subscriber
                .PublishAsync(RedisChannelName, Serialize(data))
                .ConfigureAwait(false);
        }

        #region Utilities

        protected virtual byte[] Serialize(object item)
        {
            var jsonString = JsonConvert.SerializeObject(item, _settings);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        protected virtual ArabamCacheModel Deserialize(byte[] serializedObject)
        {
            if (serializedObject == null)
                return null;

            var jsonString = Encoding.UTF8.GetString(serializedObject);

            return JsonConvert.DeserializeObject<ArabamCacheModel>(jsonString, _settings);
        }

        // ReSharper disable once InconsistentNaming
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        #endregion
    }
}