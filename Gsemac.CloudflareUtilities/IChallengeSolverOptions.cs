using System.Net;

namespace Gsemac.CloudflareUtilities {

    public interface IChallengeSolverOptions {

        IWebProxy Proxy { get; set; }
        int Timeout { get; set; }
        string UserAgent { get; set; }

    }

}