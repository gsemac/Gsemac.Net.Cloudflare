using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrChallengeSolverFactory :
        IChallengeSolverFactory {

        // Public members

        public event LogEventHandler Log;

        public FlareSolverrChallengeSolverFactory(IFlareSolverrService flareSolverrService) :
            this(flareSolverrService, ChallengeSolverOptions.Default) {
        }
        public FlareSolverrChallengeSolverFactory(IFlareSolverrService flareSolverrService, IChallengeSolverOptions solverOptions) {

            this.flareSolverrService = flareSolverrService;
            this.solverOptions = solverOptions;

        }

        public IChallengeSolver Create() {

            IChallengeSolver solver = new FlareSolverrChallengeSolver(flareSolverrService, solverOptions);

            solver.Log += Log;

            return solver;

        }

        // Protected members

        protected LogEventHandlerWrapper OnLog => new LogEventHandlerWrapper(Log, "Cloudflare IUAM Challenge Solver Factory");

        // Private members

        private readonly IFlareSolverrService flareSolverrService;
        private readonly IChallengeSolverOptions solverOptions;

    }

}