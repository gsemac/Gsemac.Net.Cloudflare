using System.Net;

namespace Gsemac.Net.CloudflareUtilities {

    public class ChallengeResponse :
        IChallengeResponse {

        // Public members

        public string UserAgent { get; }
        public CookieCollection Cookies { get; } = new CookieCollection();
        public bool Success => !string.IsNullOrEmpty(UserAgent) && Cookies.Count > 0;

        public static ChallengeResponse Failed => new ChallengeResponse();

        public ChallengeResponse(string userAgent = "", CookieCollection cookies = null) {

            UserAgent = userAgent;

            if (cookies != null)
                Cookies = cookies;

        }

        // Private members

        private ChallengeResponse() {
        }

    }

}