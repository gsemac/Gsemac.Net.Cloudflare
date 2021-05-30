using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare {

    public abstract class ChallengeSolverBase :
        IChallengeSolver {

        // Public members



        public event LogEventHandler Log;

        public string Name { get; }

        public abstract IChallengeResponse GetResponse(Uri uri);

        // Protected members

        protected LogEventHandlerWrapper OnLog => new LogEventHandlerWrapper(Log, Name);

        protected ChallengeSolverBase() :
            this("Cloudflare IUAM Challenge Solver") {
        }
        protected ChallengeSolverBase(string name) {

            Name = name;

        }

    }

}