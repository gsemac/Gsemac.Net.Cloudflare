using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal class FlareSolverrInfo :
        IFlareSolverrInfo {

        [JsonProperty("version"), JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
        [JsonProperty("executablePath")]
        public string ExecutablePath { get; set; }

    }

}