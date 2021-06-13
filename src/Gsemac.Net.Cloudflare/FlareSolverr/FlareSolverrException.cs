using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public sealed class FlareSolverrException :
        ChallengeHandlerException {

        public FlareSolverrException() {
        }
        public FlareSolverrException(string message) :
            base(message) {
        }
        public FlareSolverrException(string message, Exception innerException) :
            base(message, innerException) {
        }

    }

}