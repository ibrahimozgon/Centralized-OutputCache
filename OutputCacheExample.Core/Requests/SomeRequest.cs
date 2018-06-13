using System.Collections.Generic;

namespace OutputCacheExample.Core.Requests
{
    public class SomeRequest
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public IList<KeyValuePair<string,string>> SomeList { get; set; }
    }
}