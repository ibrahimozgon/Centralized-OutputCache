using System;
using System.Threading;
using NUnit.Framework;
using OutputCacheExample;
using OutputCacheExample.Web;
using OutputCacheExample.Web.CacheManagers;

namespace OutputCacheTests
{
    [TestFixture]
    public class KeyspaceTests
    {
        [Test]
        public void ShouldSubscribeToKeyspace()
        {
            RedisCacheManager.Instance.SubscribeToKeyspaceNotifications(null);
            var i = 0;
            while (i < 10)
            {
                RedisCacheManager.Instance.Set(new CacheModel
                {
                    Key = "test" + i,
                    CreatedAt = DateTime.Now,
                    Data = "asasd" + i,
                    Timeout = 1
                }, false);
                i++;
            }

            Thread.Sleep(int.MaxValue);
        }
    }
}