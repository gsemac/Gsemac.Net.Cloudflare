using Gsemac.Net.Cloudflare.Iuam.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Gsemac.Net.Cloudflare.Iuam {

    internal class FlareSolverrResponse {

        [JsonProperty("solution")]
        public FlareSolverrSolution Solution { get; set; } = new FlareSolverrSolution();
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("startTimestamp"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset StartTimestamp { get; set; }
        [JsonProperty("endTimestamp"), JsonConverter(typeof(MillisecondsUnixEpochConverter))]
        public DateTimeOffset EndTimestamp { get; set; }
        [JsonProperty("version"), JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; } = new Version();

    }

}