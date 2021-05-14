using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public sealed class FlareSolverrException :
        Exception {

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