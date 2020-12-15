using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class IuamChallengeResponse :
        IIuamChallengeResponse {

        // Public members

        public string UserAgent { get; }
        public CookieCollection Cookies { get; } = new CookieCollection();
        public bool Success => !string.IsNullOrEmpty(UserAgent) && Cookies.Count > 0;

        public static IuamChallengeResponse Failed => new IuamChallengeResponse();

        public IuamChallengeResponse(string userAgent = "", CookieCollection cookies = null) {

            UserAgent = userAgent;

            if (cookies != null)
                Cookies = cookies;

        }

        // Private members

        private IuamChallengeResponse() {
        }

    }

}