using Gsemac.Net.Cloudflare.FlareSolverr.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrRequest {

        [JsonProperty("cmd")]
        string Command { get; set; }
        [JsonProperty("download")]
        bool Download { get; set; }
        [JsonProperty("url")]
        Uri Url { get; set; }
        [JsonProperty("session")]
        string Session { get; set; }
        [JsonProperty("userAgent")]
        string UserAgent { get; set; }
        [JsonProperty("maxTimeout"), JsonConverter(typeof(FlareSolverrMillisecondsTimeSpanJsonConverter))]
        TimeSpan MaxTimeout { get; set; }
        [JsonProperty("headers")]
        IDictionary<string, string> Headers { get; }
        [JsonProperty("cookies"), JsonConverter(typeof(FlareSolverrCookieCollectionJsonConverter))]
        CookieCollection Cookies { get; }
        [JsonProperty("returnOnlyCookies")]
        bool ReturnOnlyCookies { get; set; }
        [JsonProperty("postData")]
        string PostData { get; set; }

    }

}