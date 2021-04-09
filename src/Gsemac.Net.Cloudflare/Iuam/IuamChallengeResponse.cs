using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class IuamChallengeResponse :
        IIuamChallengeResponse {

        // Public members

        public string UserAgent { get; set; }
        public CookieCollection Cookies { get; set; } = new CookieCollection();
        public Uri ResponseUri { get; set; }
        public string ResponseBody { get; set; } = string.Empty;
        public bool Success => !string.IsNullOrEmpty(UserAgent) && Cookies.Count > 0;

        public static IuamChallengeResponse Failed => new IuamChallengeResponse(null);

        public IuamChallengeResponse(Uri requestUri) {

            ResponseUri = requestUri;

        }

    }

}