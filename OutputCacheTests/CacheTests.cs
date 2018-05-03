using System;
using System.Threading;
using NUnit.Framework;
using OutputCacheExample;
using OutputCacheExample.CacheManagers;

namespace OutputCacheTests
{
    [TestFixture]
    public class CacheTests
    {
        private readonly string key = "some_key";
        private static Random _random = new Random();
        [Test]
        public void ShouldGetNull()
        {
            var val = RedisCacheManager.Instance.Get(key);
            Assert.AreEqual(null, val);
        }
        [Test]
        public void ShouldSetValue()
        {
            RedisCacheManager.Instance.Clear();
            RedisCacheManager.Instance.Set(GetDefaultValue());
        }

        private ArabamCacheModel GetDefaultValue()
        {
            return new ArabamCacheModel
            {
                Key = key,
                Data = new SomeData
                {
                    Order = 1,
                    Value = key
                }
            };
        }
        private ArabamCacheModel GetRandomVal()
        {
            return new ArabamCacheModel
            {
                Key = key + _random.Next(),
                Data = new SomeData
                {
                    Order = _random.Next(),
                    Value = key + _random.Next(),
                },
                Timeout = 10
            };
        }

        [Test]
        public void ShouldSetAndGetValue()
        {
            RedisCacheManager.Instance.Clear();
            RedisCacheManager.Instance.Set(GetDefaultValue());
            var val = RedisCacheManager.Instance.Get(key);
            Assert.NotNull(val);
            var expected = new SomeData
            {
                Order = 1,
                Value = key
            };
            var converted = (SomeData)val.Data;
            Assert.AreEqual(expected.Order, converted.Order);
            Assert.AreEqual(expected.Value, converted.Value);
        }

        [Test]
        public void ShouldSubscribeToRedis()
        {
            RedisCacheManager.Instance.Clear();
            RedisCacheManager.Instance.Subscribe(Consumer);
            RedisCacheManager.Instance.Set(GetDefaultValue());
            Console.ReadLine();
        }

        [Test]
        public void ShouldSubscribeToRedisMany()
        {
            RedisCacheManager.Instance.Clear();
            RedisCacheManager.Instance.Subscribe(Consumer);
            for (int i = 0; i < 100; i++)
            {
                var val = GetRandomVal();
                RedisCacheManager.Instance.Set(GetRandomVal());
            }

            Thread.Sleep(10000);
        }

        private static void Consumer(ArabamCacheModel obj)
        {
            Console.WriteLine(obj.Data);
            Console.WriteLine(obj.Key);
        }

        public class SomeData
        {
            public string Value { get; set; }
            public int Order { get; set; }
        }
    }
}
