using System;
using System.Collections.Generic;
using System.Text;

namespace Gsemac.CloudflareUtilities {

    public class ChallengeResponse :
        IChallengeResponse {

        // Public members

        public string UserAgent { get; }
        public IDictionary<string, string> Cookies { get; } = new Dictionary<string, string>();
        public bool Success { get; } = false;

        public static ChallengeResponse Failed => new ChallengeResponse();

        public ChallengeResponse(bool success, string userAgent = "", IDictionary<string, string> cookies = null) {

            this.UserAgent = userAgent;
            this.Success = success;

            if (cookies != null)
                this.Cookies = cookies;

        }

        // Private members

        private ChallengeResponse() { }

    }

}