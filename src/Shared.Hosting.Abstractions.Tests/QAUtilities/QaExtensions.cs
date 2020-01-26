using System.Collections.Generic;

namespace Shared.Hosting.Abstractions.Tests.QAUtilities
{
    public static class QaExtensions
    {
        public static string[] ToConfigurationArgs(this ICollection<KeyValuePair<string, string>> dic)
        {
            var result = new string[dic.Count];

            var i = 0;

            foreach (var pair in dic)
                result[i++] = $"{pair.Key}={pair.Value}";

            return result;
        }
        public static string[] ToConfigurationArgs(this object input)
        {
            return JsonConfigurationParser.Parse(input).ToConfigurationArgs();
        }
    }
}