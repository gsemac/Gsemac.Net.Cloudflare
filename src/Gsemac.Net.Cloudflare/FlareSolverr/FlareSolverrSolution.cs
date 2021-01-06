using Gsemac.Net.Cloudflare.Iuam.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal class FlareSolverrSolution {

        [JsonProperty("url")]
        public Uri Url { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("headers")]
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        [JsonProperty("response")]
        public string Response { get; set; }
        [JsonProperty("cookies"), JsonConverter(typeof(FlareSolverrDataCookieCollectionJsonConverter))]
        public CookieCollection Cookies { get; set; } = new CookieCollection();
        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

    }

}