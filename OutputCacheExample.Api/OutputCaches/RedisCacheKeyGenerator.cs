using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OutputCacheExample.Core.Helpers;
using WebApi.OutputCache.V2;

namespace OutputCacheExample.Api.OutputCaches
{
    public class RedisCacheKeyGenerator : DefaultCacheKeyGenerator
    {
        public override string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
        {
            var baseKey = MakeBaseKey(context);
            var mediaTypeAsStr = mediaType.ToString().Replace(";", " ").Replace(" ", "");
            var parameters = FormatParameters(context, excludeQueryString);
            if (string.IsNullOrEmpty(parameters))
                return $"{baseKey}:{mediaTypeAsStr}";

            return $"{baseKey}:{parameters}:{mediaTypeAsStr}";
        }

        protected override string MakeBaseKey(HttpActionContext context)
        {
            return $"ApiOutputCache:{context.ControllerContext.ControllerDescriptor.ControllerType.Name}:{context.ActionDescriptor.ActionName}";
        }

        protected override string FormatParameters(HttpActionContext context, bool excludeQueryString)
        {
            var parametersAsString = base.FormatParameters(context, excludeQueryString).ReplaceFirst("-", "");
            if (excludeQueryString || context.Request.Method != HttpMethod.Post)
                return parametersAsString;

            var postBody = GetPostBody(context);
            if (string.IsNullOrEmpty(postBody))
                return parametersAsString;

            var postParameters = JObject.Parse(postBody);
            if (postParameters == null)
                return parametersAsString;
            parametersAsString += FormatPostParameters(postParameters);
            return parametersAsString;
        }

        private static string FormatPostParameters(JObject postParameters)
        {
            var parametersAsString = ":";
            foreach (var param in postParameters)
            {
                var val = param.Value?.ToString(Formatting.None);
                if (string.IsNullOrEmpty(param.Key) || string.IsNullOrEmpty(val))
                    continue;

                val = val.Replace(Environment.NewLine, "")
                    .Replace(":", "=")
                    .Replace("{", "")
                    .Replace("}", "")
                    .Replace(",", "_")
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("\"", "")
                    .Trim()
                    .ToLower();
                if (string.IsNullOrEmpty(val))
                    continue;

                parametersAsString += param.Key.ToLower() + "=" + val + ":";
            }

            return parametersAsString.TrimEnd(':');
        }

        private static string GetPostBody(HttpActionContext context)
        {
            string postBody;
            using (var stream = new MemoryStream())
            {
                var inputStream = context.Request.Content.ReadAsStreamAsync().Result;
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(stream);
                postBody = Encoding.UTF8.GetString(stream.ToArray());
            }

            return postBody;
        }
    }
}