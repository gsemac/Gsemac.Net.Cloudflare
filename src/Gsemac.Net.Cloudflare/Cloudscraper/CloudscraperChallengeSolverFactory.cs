using Gsemac.IO.Logging;

namespace Gsemac.Net.Cloudflare.Cloudscraper {

    public class CloudscraperChallengeSolverFactory :
        IChallengeSolverFactory {

        // Public members

        public event LogEventHandler Log;

        public CloudscraperChallengeSolverFactory(ICloudscraperOptions cloudscraperOptions) :
            this(cloudscraperOptions, ChallengeSolverOptions.Default) {
        }
        public CloudscraperChallengeSolverFactory(ICloudscraperOptions cloudscraperOptions, IChallengeSolverOptions solverOptions) {

            this.cloudscraperOptions = cloudscraperOptions;
            this.solverOptions = solverOptions;

        }

        public IChallengeSolver Create() {

            IChallengeSolver solver = new CloudscraperChallengeSolver(cloudscraperOptions, solverOptions);

            solver.Log += Log;

            return solver;

        }

        // Private members

        private readonly ICloudscraperOptions cloudscraperOptions;
        private readonly IChallengeSolverOptions solverOptions;

    }

}
