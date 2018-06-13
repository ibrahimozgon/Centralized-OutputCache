using System;
using System.Text;
using System.Web.Caching;
using OutputCacheExample.Core.CacheManagers;

namespace OutputCacheExample.Web.CacheProviders
{
    public class CentralizedOutputCacheProvider : OutputCacheProvider
    {
        public override object Get(string key)
        {
            return RedisCacheManager.Instance.Get<object>(PrepareCacheKey(key));
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            var existingValue = Get(key);
            if (existingValue != null)
                return existingValue;

            Set(key, entry, utcExpiry);
            return entry;
        }

        public override void Set(string key, object data, DateTime utcExpiry)
        {
            var expiry = (int)(utcExpiry - DateTime.UtcNow).TotalMinutes;
            if (expiry <= 0)
                expiry = 1;

            RedisCacheManager.Instance.Set(PrepareCacheKey(key), data, expiry);
        }

        public override void Remove(string key)
        {
            RedisCacheManager.Instance.Remove(PrepareCacheKey(key));
        }

        private static string PrepareCacheKey(string key)
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append("WebOutputCache:");
            key = key.Replace("_d0nutc@che.", "").Replace("#", ":").TrimEnd(':');
            strBuilder.Append(key);
            return strBuilder.ToString();
        }
    }
}