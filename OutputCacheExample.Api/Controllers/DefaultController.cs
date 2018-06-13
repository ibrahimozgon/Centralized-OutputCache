using System.Collections.Generic;
using System.Web.Http;
using OutputCacheExample.Api.OutputCaches;
using OutputCacheExample.Core.Requests;

namespace OutputCacheExample.Api.Controllers
{
    public class DefaultController : ApiController
    {
        [ArabamApiOutputCache("Long")]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        [ArabamApiOutputCache("Medium")]
        public string Get(int id)
        {
            return "value";
        }
        private static readonly IList<SomeRequest> _lists = new List<SomeRequest>();

        [ArabamApiOutputCache("Medium")]
        public IList<SomeRequest> Post([FromBody]SomeRequest request)
        {
            _lists.Add(request);
            return _lists;
        }
    }
}
