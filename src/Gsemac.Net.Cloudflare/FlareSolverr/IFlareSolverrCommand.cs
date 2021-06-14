using Gsemac.Net.Cloudflare.FlareSolverr.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrCommand {

        [JsonProperty("cmd")]
        string Cmd { get; }
        [JsonProperty("download")]
        bool Download { get; }
        [JsonProperty("url")]
        Uri Url { get; }
        [JsonProperty("session")]
        string Session { get; }
        [JsonProperty("userAgent")]
        string UserAgent { get; }
        [JsonProperty("maxTimeout"), JsonConverter(typeof(FlareSolverrMillisecondsTimeSpanJsonConverter))]
        TimeSpan MaxTimeout { get; }
        [JsonProperty("headers")]
        IDictionary<string, string> Headers { get; }
        [JsonProperty("cookies"), JsonConverter(typeof(FlareSolverrCookieCollectionJsonConverter))]
        CookieCollection Cookies { get; }
        [JsonProperty("returnOnlyCookies")]
        bool ReturnOnlyCookies { get; }
        [JsonProperty("postData")]
        string PostData { get; }

    }

}