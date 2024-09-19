using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gsemac.Cloudflare.CFClearanceScraper {

    public interface ICfClearanceScraperResponse {

        [JsonProperty("code")]
        int StatusCode { get; }
        [JsonProperty("cookies")]
        CookieCollection Cookies { get; }
        [JsonProperty("agent")]
        string UserAgent { get; }
        [JsonProperty("proxy")]
        IWebProxy Proxy { get; }
        [JsonProperty("url")]
        Uri Url { get; }
        [JsonProperty("headers")]
        IDictionary<string, string> Headers { get; }

    }

}