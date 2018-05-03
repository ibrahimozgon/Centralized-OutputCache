using System;
using System.Collections.Specialized;
using System.Runtime.Caching;

namespace OutputCacheExample.CacheManagers
{
    public class MemoryCacheManager
    {
        private readonly ObjectCache _cache;
        private static readonly Lazy<MemoryCacheManager> Lazy =
            new Lazy<MemoryCacheManager>(() => new MemoryCacheManager());

        public static MemoryCacheManager Instance => Lazy.Value;

        private MemoryCacheManager()
        {
            _cache = new MemoryCache("arabamOutputCache", new NameValueCollection());
        }

        public ArabamCacheModel Get(string key)
        {
            if (IsSet(key))
                return (ArabamCacheModel)_cache[key];
            return null;
        }
        public void Set(ArabamCacheModel data)
        {
            if (data == null)
                return;

            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(data.Timeout) };
            _cache.Add(new CacheItem(data.Key, data), policy);
        }

        public bool IsSet(string key)
        {
            return _cache.Contains(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            foreach (var item in _cache)
                Remove(item.Key);
        }

    }
}