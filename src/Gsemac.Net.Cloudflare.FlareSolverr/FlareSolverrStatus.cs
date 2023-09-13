using Gsemac.Net.Cloudflare.FlareSolverr.Json;
using Newtonsoft.Json;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal sealed class FlareSolverrStatus :
        IFlareSolverrStatus {

        // Public members

        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; } = string.Empty;
        [JsonConverter(typeof(FlareSolverrVersionConverter))]
        public Version Version { get; set; } = new Version();
        public string UserAgent { get; set; } = string.Empty;

    }

}