using System;
using System.Web.Caching;
using OutputCacheExample.CacheManagers;

namespace OutputCacheExample
{
    public class DistributedOutputCacheProvider : OutputCacheProvider
    {
        public DistributedOutputCacheProvider()
        {
            RedisCacheManager.Instance.Subscribe(Consumer);
        }

        private static void Consumer(CacheModel data)
        {
            if (data == null)
                return;

            if (data.Data == null)
                MemoryCacheManager.Instance.Remove(data.Key);
            else
                MemoryCacheManager.Instance.Set(data);
        }
        public override object Get(string key)
        {
            if (MemoryCacheManager.Instance.IsSet(key))
                return MemoryCacheManager.Instance.Get(key)?.Data;

            if (RedisCacheManager.Instance.IsSet(key))
                return RedisCacheManager.Instance.Get(key)?.Data;
            return null;
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            var existingValue = Get(key);
            if (existingValue != null)
                return existingValue;

            Set(key, entry, utcExpiry);
            return entry;
        }

        public override void Set(string key, object entry, DateTime utcExpiry)
        {
            var expiry = (int)((utcExpiry - DateTime.UtcNow).TotalMinutes);
            if (expiry <= 0)
                expiry = 1;

            var model = new CacheModel
            {
                Data = entry,
                Key = key,
                Timeout = expiry,
            };

            RedisCacheManager.Instance.Set(model);

            MemoryCacheManager.Instance.Set(model);
        }

        public override void Remove(string key)
        {
            RedisCacheManager.Instance.Remove(key);
            MemoryCacheManager.Instance.Remove(key);
        }
    }
}