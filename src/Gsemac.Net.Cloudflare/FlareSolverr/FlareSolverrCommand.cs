using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrCommand :
        IFlareSolverrCommand {

        public string Cmd { get; set; }
        public bool Download { get; set; } = false;
        public Uri Url { get; set; }
        public string UserAgent { get; set; }
        public TimeSpan MaxTimeout { get; set; } = TimeSpan.FromSeconds(60);
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public CookieCollection Cookies { get; set; } = new CookieCollection();
        public string Session { get; set; }
        public bool ReturnOnlyCookies { get; set; } = false;
        public string PostData { get; set; }

        public FlareSolverrCommand(string cmd) {

            this.Cmd = cmd;

        }

        public override string ToString() {

            return JsonConvert.SerializeObject(this, new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            });

        }

    }

}