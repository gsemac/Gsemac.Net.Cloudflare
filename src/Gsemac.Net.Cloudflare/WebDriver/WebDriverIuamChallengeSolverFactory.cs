using Gsemac.IO.Logging;
using Gsemac.Net.Cloudflare.Iuam;
using Gsemac.Net.WebDrivers;

namespace Gsemac.Net.Cloudflare.WebDriver {

    public class WebDriverIuamChallengeSolverFactory :
        IIuamChallengeSolverFactory {

        // Public members

        public event LogEventHandler Log;

        public WebDriverIuamChallengeSolverFactory(IWebDriverFactory webDriverFactory, IIuamChallengeSolverOptions solverOptions) {

            this.webDriverFactory = webDriverFactory;
            this.solverOptions = solverOptions;

        }

        public IIuamChallengeSolver Create() {

            IIuamChallengeSolver solver = new WebDriverIuamChallengeSolver(webDriverFactory, solverOptions);

            solver.Log += Log;

            return solver;

        }

        // Private members

        private readonly IWebDriverFactory webDriverFactory;
        private readonly IIuamChallengeSolverOptions solverOptions;

    }

}