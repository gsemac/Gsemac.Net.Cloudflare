using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    internal class IuamChallengeSolverHttpWebRequest :
        HttpWebRequestBase {

        // Public members

        public IuamChallengeSolverHttpWebRequest(Uri requestUri, WebException originalException, IIuamChallengeSolver challengeSolver) :
            base(requestUri) {

            this.challengeSolver = challengeSolver;
            this.originalException = originalException;

        }

        public override WebResponse GetResponse() {

            IIuamChallengeResponse challengeResponse = challengeSolver.GetResponse(RequestUri);

            return new IuamChallengeSolverHttpWebResponse(RequestUri, originalException, challengeResponse);

        }

        // Private members

        private readonly IIuamChallengeSolver challengeSolver;
        private readonly WebException originalException;

    }

}