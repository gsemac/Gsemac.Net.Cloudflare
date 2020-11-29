using Gsemac.Net.WebDrivers;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.WebDrivers {

    public interface IWebDriverIuamChallengeSolverOptions :
        IIuamChallengeSolverOptions,
        IWebDriverOptions {

        new IWebProxy Proxy { get; set; }
        new TimeSpan Timeout { get; set; }
        new string UserAgent { get; set; }

    }

}