using Gsemac.Cloudflare.CFClearanceScraper.Json;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Gsemac.Cloudflare.CFClearanceScraper {

    public interface ICfClearanceScraperRequest {

        [JsonProperty("authToken")]
        string AuthToken { get; }
        [JsonProperty("url")]
        Uri Url { get; }
        [JsonProperty("mode"), JsonConverter(typeof(CfClearanceScraperModeJsonConverter))]
        CfClearanceScraperMode Mode { get; }
        [JsonProperty("agent")]
        string UserAgent { get; }
        [JsonProperty("proxy")]
        IWebProxy Proxy { get; }

    }

}