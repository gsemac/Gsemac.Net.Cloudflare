using Gsemac.IO.Logging;
using Gsemac.Net.Cloudflare.Iuam;

namespace Gsemac.Net.Cloudflare.Cloudscraper {

    public class CloudscraperIuamChallengeSolverFactory :
        IIuamChallengeSolverFactory {

        // Public members

        public event LogEventHandler Log;

        public CloudscraperIuamChallengeSolverFactory(ICloudscraperOptions cloudscraperOptions) :
            this(cloudscraperOptions, IuamChallengeSolverOptions.Default) {
        }
        public CloudscraperIuamChallengeSolverFactory(ICloudscraperOptions cloudscraperOptions, IIuamChallengeSolverOptions solverOptions) {

            this.cloudscraperOptions = cloudscraperOptions;
            this.solverOptions = solverOptions;

        }

        public IIuamChallengeSolver Create() {

            IIuamChallengeSolver solver = new CloudscraperIuamChallengeSolver(cloudscraperOptions, solverOptions);

            solver.Log += Log;

            return solver;

        }

        // Private members

        private readonly ICloudscraperOptions cloudscraperOptions;
        private readonly IIuamChallengeSolverOptions solverOptions;

    }

}
