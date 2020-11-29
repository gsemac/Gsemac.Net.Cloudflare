using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare {

    public abstract class IuamChallengeSolverBase :
        IIuamChallengeSolver {

        // Public members

        public event LogEventHandler Log;

        public abstract IIuamChallengeResponse GetChallengeResponse(Uri uri);

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper(sourceName, Log);

        protected IuamChallengeSolverBase() :
            this("CF Challenge Solver") {
        }
        protected IuamChallengeSolverBase(string sourceName) {

            this.sourceName = sourceName;

        }

        // Private members

        private readonly string sourceName;

    }

}