using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspNetFileNicluder.Logic.Util.StaticExtensions;

namespace AspNetFileNicluder.Logic.Util
{
    public class SettingsValueResolver
    {
        public static string Resolve(string value, Settings settings)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var patterns = GetValuePatterns(value);
            var dataValues = ResolvePatterns(patterns, settings);
            foreach(var item in dataValues)
            {
                var odlValue = "{{" + item.Key + "}}";
                value = value.Replace(odlValue, item.Value);
            }

            return value;
        }

        private static IDictionary<string, string> ResolvePatterns(IList<string> patterns, Settings settings)
        {
            var results = new Dictionary<string, string>();
            foreach(var pattern in patterns)
            {
                if (results.ContainsKey(pattern)) continue;

                switch (pattern)
                {
                    case string dateTime when pattern.StartsWith("DateTime:"):
                        results.Add(pattern, FromatDateTime(pattern));
                        break;
                    default:
                        var a = settings.Constants.ContainsKey(pattern) ? settings.Constants[pattern] : string.Empty;
                        results.Add(pattern, a);
                        break;
                }
            }

            return results;
        }

        private static string FromatDateTime(string pattern)
        {
            DateTime value = DateTime.Now;
            if(pattern.Contains(":format("))
            {
                pattern = pattern.Replace("DateTime:format(", string.Empty).Replace(")", string.Empty);
                return value.ToString(pattern);
            }

            return value.ToString();
        }

        private static IList<string> GetValuePatterns(string text)
        {
            var patterns = new List<string>();
            var startIndex = text.IndexOf("{{");
            var endIndex = text.IndexOf("}}");

            if (startIndex == -1 || endIndex == -1 || startIndex >= endIndex) return patterns;


            patterns.Add(text.Substring(startIndex, endIndex - startIndex).TrimStart('{').TrimEnd('}'));

            if(text.Length > endIndex + 2) {
                var leftString = text.Substring(endIndex + 2);
                patterns.AddRange(GetValuePatterns(leftString));
            }

            return patterns;
        }
    }
}
