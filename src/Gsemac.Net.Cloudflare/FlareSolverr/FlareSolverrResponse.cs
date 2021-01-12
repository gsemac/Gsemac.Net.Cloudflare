using Gsemac.Net.Cloudflare.FlareSolverr.Json;
using Newtonsoft.Json;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal class FlareSolverrResponse {

        [JsonProperty("solution")]
        public FlareSolverrSolution Solution { get; set; } = new FlareSolverrSolution();
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("startTimestamp"), JsonConverter(typeof(FlareSolverrDataMillisecondsUnixEpochConverter))]
        public DateTimeOffset StartTimestamp { get; set; }
        [JsonProperty("endTimestamp"), JsonConverter(typeof(FlareSolverrDataMillisecondsUnixEpochConverter))]
        public DateTimeOffset EndTimestamp { get; set; }
        // Version strings are of the form "v1.2.3", which the built-in version converter can't parse
        [JsonProperty("version"), JsonConverter(typeof(FlareSolverrDataVersionConverter))]
        public Version Version { get; set; } = new Version();

    }

}