using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

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

        public static SequenceValue FromCookies(IEnumerable<ResponseCookie> cookies) 
        {
            return new SequenceValue(cookies.Select(cookie => 
            {
                var pairs = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>();
                pairs.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>
                (
                    new ScalarValue(nameof(cookie.Name)), 
                    new ScalarValue(cookie.Name)
                ));
				pairs.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>
                (
                	new ScalarValue(nameof(cookie.Value)),
                	new ScalarValue(cookie.Value)
                ));
				pairs.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>
				(
					new ScalarValue(nameof(cookie.HttpOnly)),
					new ScalarValue(cookie.HttpOnly)
				));
				pairs.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>
				(
					new ScalarValue(nameof(cookie.Expires)),
					new ScalarValue(cookie.Expires)
				));
				pairs.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>
				(
					new ScalarValue(nameof(cookie.Secure)),
					new ScalarValue(cookie.Secure)
				));
                return new DictionaryValue(pairs);
            }));
        }
    }
}
