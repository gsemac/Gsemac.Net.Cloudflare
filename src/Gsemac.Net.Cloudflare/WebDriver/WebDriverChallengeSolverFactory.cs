using Gsemac.IO.Logging;
using Gsemac.Net.WebDrivers;

namespace Gsemac.Net.Cloudflare.WebDriver {

    public class WebDriverChallengeSolverFactory :
        IChallengeSolverFactory {

        // Public members

        public event LogEventHandler Log;

        public WebDriverChallengeSolverFactory(IWebDriverFactory webDriverFactory) :
            this(webDriverFactory, ChallengeSolverOptions.Default) {
        }
        public WebDriverChallengeSolverFactory(IWebDriverFactory webDriverFactory, IChallengeSolverOptions solverOptions) {

            this.webDriverFactory = webDriverFactory;
            this.solverOptions = solverOptions;

        }

        public IChallengeSolver Create() {

            IChallengeSolver solver = new WebDriverChallengeSolver(webDriverFactory, solverOptions);

            solver.Log += Log;

            return solver;

        }

        // Private members

        private readonly IWebDriverFactory webDriverFactory;
        private readonly IChallengeSolverOptions solverOptions;

    }

}