using Gsemac.Net.Cloudflare.Iuam.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal class FlareSolverrData {

        [JsonProperty("cmd")]
        public string Cmd { get; set; } = "request.get";
        [JsonProperty("url")]
        public Uri Url { get; set; }
        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }
        [JsonProperty("maxTimeout"), JsonConverter(typeof(MillisecondsTimeSpanJsonConverter))]
        public TimeSpan MaxTimeout { get; set; } = TimeSpan.FromSeconds(60);
        [JsonProperty("headers")]
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        [JsonProperty("cookies"), JsonConverter(typeof(FlareSolverrDataCookieCollectionJsonConverter))]
        public CookieCollection Cookies { get; set; } = new CookieCollection();

        public override string ToString() {

            return JsonConvert.SerializeObject(this, new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            });

        }

    }

}