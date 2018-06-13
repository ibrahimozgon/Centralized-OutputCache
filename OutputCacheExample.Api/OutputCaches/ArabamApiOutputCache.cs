using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using WebApi.OutputCache.V2;

namespace OutputCacheExample.Api.OutputCaches
{
    public class ArabamApiOutputCache : CacheOutputAttribute
    {
        private readonly bool _isCacheProfileNotFound;
        public ArabamApiOutputCache()
        {
            
        }
        public ArabamApiOutputCache(string cacheProfile)
        {
            var profile = CacheProfileHelper.CacheProfiles[cacheProfile];
            if (profile?.Enabled != true || profile.Duration <= 0)
            {
                _isCacheProfileNotFound = true;
                return;
            }
            ServerTimeSpan = profile.Duration;
            ClientTimeSpan = profile.Duration;
        }

        protected override bool IsCachingAllowed(HttpActionContext actionContext, bool anonymousOnly)
        {
            if (_isCacheProfileNotFound)
                return false;

            if (anonymousOnly && Thread.CurrentPrincipal.Identity.IsAuthenticated)
                return false;

            if (actionContext.ActionDescriptor.GetCustomAttributes<IgnoreCacheOutputAttribute>().Any())
                return false;

            return actionContext.Request.Method == HttpMethod.Get || actionContext.Request.Method == HttpMethod.Post;//Postu biz ekledik.
        }
    }
}