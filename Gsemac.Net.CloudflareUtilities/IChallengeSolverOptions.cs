using System;
using System.Net;

namespace Gsemac.Net.CloudflareUtilities {

    public interface IChallengeSolverOptions {

        IWebProxy Proxy { get; set; }
        TimeSpan Timeout { get; set; }
        string UserAgent { get; set; }

    }

}