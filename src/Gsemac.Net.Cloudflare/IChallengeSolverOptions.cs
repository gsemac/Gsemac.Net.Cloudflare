using System;
using System.Net;

namespace Gsemac.Net.Cloudflare {

    public interface IChallengeSolverOptions {

        IWebProxy Proxy { get; set; }
        TimeSpan Timeout { get; set; }
        string UserAgent { get; set; }

    }

}