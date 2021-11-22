using System.Net;

namespace Gsemac.Net.Cloudflare {

    internal class ChallengeSolution :
        IChallengeSolution {

        // Public members

        public string UserAgent { get; }
        public CookieCollection Cookies { get; }

        public ChallengeSolution(string userAgent, CookieCollection cookies) {

            UserAgent = userAgent;
            Cookies = cookies;

        }

    }

}