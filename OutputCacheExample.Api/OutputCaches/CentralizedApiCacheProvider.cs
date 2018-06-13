using System;
using System.Collections.Generic;
using OutputCacheExample.Core.CacheManagers;
using WebApi.OutputCache.Core.Cache;

namespace OutputCacheExample.Api.OutputCaches
{
    public class CentralizedApiCacheProvider : IApiOutputCache
    {
        public virtual void RemoveStartsWith(string key)
        {
            RedisCacheManager.Instance.Remove(key);
        }
        public virtual T Get<T>(string key) where T : class
        {
            return RedisCacheManager.Instance.Get<T>(key);
        }
        public virtual object Get(string key)
        {
            return Get<object>(key);
        }
        public virtual void Remove(string key)
        {
            RedisCacheManager.Instance.Remove(key);
        }
        public virtual bool Contains(string key)
        {
            return RedisCacheManager.Instance.IsSet(key);
        }
        public virtual void Add(string key, object data, DateTimeOffset utcExpiry, string dependsOnKey = null)
        {
            var expiry = (int)(utcExpiry - DateTime.UtcNow).TotalMinutes;
            if (expiry <= 0)
                expiry = 1;
            if (data == null || data.ToString() == "")
                return;

            RedisCacheManager.Instance.Set(key, data, expiry);
        }

        public virtual IEnumerable<string> AllKeys => throw new NotImplementedException();
    }
}