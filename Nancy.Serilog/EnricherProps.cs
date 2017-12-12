using Serilog.Events;
using System.Collections.Generic;

namespace Nancy.Serilog
{
    public static class EnricherProps
    {
        public static DictionaryValue FromDictionary(Dictionary<string, string> dict)
        {
            var pairs = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>();
            foreach (var pair in dict)
            {
                var key = new ScalarValue(pair.Key);
                var value = new ScalarValue(pair.Value);
                pairs.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>(key, value));
            }

            return new DictionaryValue(pairs);
        }
    }
}
