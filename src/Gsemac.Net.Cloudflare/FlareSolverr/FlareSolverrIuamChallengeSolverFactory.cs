using Gsemac.IO.Logging;
using Gsemac.Net.Cloudflare.Iuam;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrIuamChallengeSolverFactory :
        IIuamChallengeSolverFactory {

        // Public members

        public event LogEventHandler Log;

        public FlareSolverrIuamChallengeSolverFactory(IFlareSolverrService flareSolverrService) :
            this(flareSolverrService, IuamChallengeSolverOptions.Default) {
        }
        public FlareSolverrIuamChallengeSolverFactory(IFlareSolverrService flareSolverrService, IIuamChallengeSolverOptions solverOptions) {

            this.flareSolverrService = flareSolverrService;
            this.solverOptions = solverOptions;

        }

        public IIuamChallengeSolver Create() {

            IIuamChallengeSolver solver = new FlareSolverrIuamChallengeSolver(flareSolverrService, solverOptions);

            solver.Log += Log;

            return solver;

        }

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper("Cloudflare IUAM Challenge Solver Factory", Log);

        // Private members

        private readonly IFlareSolverrService flareSolverrService;
        private readonly IIuamChallengeSolverOptions solverOptions;

    }

}