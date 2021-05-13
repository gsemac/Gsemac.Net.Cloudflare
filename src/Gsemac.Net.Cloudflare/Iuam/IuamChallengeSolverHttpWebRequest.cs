using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    internal class IuamChallengeSolverHttpWebRequest :
        HttpWebRequestBase {

        // Public members

        public IuamChallengeSolverHttpWebRequest(Uri requestUri, IIuamChallengeSolver challengeSolver) :
            base(requestUri) {

            this.challengeSolver = challengeSolver;

        }

        public override WebResponse GetResponse() {

            IIuamChallengeResponse challengeResponse = challengeSolver.GetResponse(RequestUri);

            return new IuamChallengeSolverHttpWebResponse(RequestUri, challengeResponse);

        }

        // Private members

        private readonly IIuamChallengeSolver challengeSolver;

    }

}