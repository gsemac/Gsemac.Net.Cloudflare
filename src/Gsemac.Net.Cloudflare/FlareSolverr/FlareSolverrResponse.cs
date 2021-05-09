using System;
using System.Collections.Generic;
using System.Linq;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrResponse :
        IFlareSolverrResponse {

        public FlareSolverrSolution Solution { get; set; } = new FlareSolverrSolution();
        public string Session { get; set; } 
        public IEnumerable<string> Sessions { get; set; } = Enumerable.Empty<string>();
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }
        public Version Version { get; set; } = new Version();

    }

}