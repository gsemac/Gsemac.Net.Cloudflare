using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.Iuam {

    public abstract class IuamChallengeSolverBase :
        IIuamChallengeSolver {

        // Public members

        public event LogEventHandler Log;

        public string Name { get; }

        public abstract IIuamChallengeResponse GetResponse(Uri uri);

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper(Name, Log);

        protected IuamChallengeSolverBase() :
            this("Cloudflare IUAM Challenge Solver") {
        }
        protected IuamChallengeSolverBase(string name) {

            this.Name = name;

        }

    }

}