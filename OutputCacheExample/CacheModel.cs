using System;

namespace OutputCacheExample
{
    public class CacheModel
    {
        public CacheModel()
        {
            CreatedAt = DateTime.Now;
        }

        public string Key { get; set; }
        public object Data { get; set; }
        public int Timeout { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}