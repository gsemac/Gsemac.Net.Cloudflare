using System.Net;

namespace Gsemac.Net.Cloudflare {

    public interface IChallengeSolution {

        string UserAgent { get; }
        CookieCollection Cookies { get; }

    }

}