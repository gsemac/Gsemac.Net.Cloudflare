using System;

namespace Gsemac.Net.Cloudflare {

    public class ChallengeHandlerException :
        Exception {

        // Public members

        public ChallengeHandlerException() {
        }
        public ChallengeHandlerException(string message) :
            base(message) {
        }
        public ChallengeHandlerException(string message, Exception innerException) :
            base(message, innerException) {
        }

    }

}