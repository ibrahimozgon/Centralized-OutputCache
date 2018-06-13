using System.Text.RegularExpressions;

namespace OutputCacheExample.Core.Helpers
{
    public static class StringHelper
    {
        public static string ReplaceFirst(this string text, string oldValue, string newValue)
        {
            var regex = new Regex(Regex.Escape(oldValue ?? ""));
            var newText = regex.Replace(text ?? "", newValue ?? "", 1);
            return newText;
        }
    }
}