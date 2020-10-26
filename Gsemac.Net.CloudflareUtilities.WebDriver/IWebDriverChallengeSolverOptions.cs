using Gsemac.Net.SeleniumUtilities;
using System;
using System.Net;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public interface IWebDriverChallengeSolverOptions :
        IChallengeSolverOptions,
        IWebDriverOptions {

        new IWebProxy Proxy { get; set; }
        new TimeSpan Timeout { get; set; }
        new string UserAgent { get; set; }

    }

}