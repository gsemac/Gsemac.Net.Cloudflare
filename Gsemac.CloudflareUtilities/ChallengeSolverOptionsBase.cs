using System;
using System.Net;

namespace Gsemac.CloudflareUtilities {

    public abstract class ChallengeSolverOptionsBase :
        IChallengeSolverOptions {

        public IWebProxy Proxy { get; set; }
        public int Timeout { get; set; } = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
        public string UserAgent { get; set; }

    }

}