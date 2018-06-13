using System.Web.Http;
using OutputCacheExample.Api.OutputCaches;
using WebApi.OutputCache.V2;

namespace OutputCacheExample.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RegisterCache();
        }
        private static void RegisterCache()
        {
            var cacheGenerator = new RedisCacheKeyGenerator();
            var cacheProvider = new CentralizedApiCacheProvider();
            GlobalConfiguration.Configuration.CacheOutputConfiguration().RegisterDefaultCacheKeyGeneratorProvider(() => cacheGenerator);
            GlobalConfiguration.Configuration.CacheOutputConfiguration().RegisterCacheOutputProvider(() => cacheProvider);
        }
    }
}
