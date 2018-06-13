using System.Configuration;
using System.Web.Configuration;

namespace OutputCacheExample.Api.OutputCaches
{
    /// <summary>
    ///     CacheProfileHelper
    /// </summary>
    public static class CacheProfileHelper
    {
        /// <summary>
        ///     CacheProfileHelper
        /// </summary>
        static CacheProfileHelper()
        {
            SetCacheProfiles();
        }

        private static void SetCacheProfiles()
        {
            const string outputCacheSettingsKey = "system.web/caching/outputCacheSettings";

            var outputCacheSettingsSection = ConfigurationManager.GetSection(outputCacheSettingsKey) as OutputCacheSettingsSection;

            CacheProfiles = outputCacheSettingsSection?.OutputCacheProfiles;
        }

        /// <summary>
        ///     CacheProfiles
        /// </summary>
        public static OutputCacheProfileCollection CacheProfiles { get; set; }
    }
}