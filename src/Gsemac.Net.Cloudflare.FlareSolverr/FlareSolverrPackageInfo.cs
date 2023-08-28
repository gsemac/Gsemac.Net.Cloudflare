using Newtonsoft.Json;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal class FlareSolverrPackageInfo :
        IFlareSolverrPackageInfo {

        // Public members

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("license")]
        public string License { get; set; }

    }

}