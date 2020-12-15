using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IIuamChallengeSolverOptions {

        IWebProxy Proxy { get; set; }
        TimeSpan Timeout { get; set; }
        string UserAgent { get; set; }

    }

}