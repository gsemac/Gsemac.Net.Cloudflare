using System.Collections.Generic;
using System.Linq;

namespace Gsemac.Net.CloudflareUtilities {

    public class ChallengeResponse :
        IChallengeResponse {

        // Public members

        public string UserAgent { get; }
        public IDictionary<string, string> Cookies { get; } = new Dictionary<string, string>();
        public bool Success => !string.IsNullOrEmpty(UserAgent) && Cookies.Any();

        public static ChallengeResponse Failed => new ChallengeResponse();

        public ChallengeResponse(string userAgent = "", IDictionary<string, string> cookies = null) {

            UserAgent = userAgent;

            if (cookies != null)
                Cookies = cookies;

        }

        // Private members

        private ChallengeResponse() {
        }

    }

}