using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.Cloudflare.WebDriver {

    public class WebDriverChallengeSolver :
          WebDriverChallengeSolverBase {

        // Public members

        public WebDriverChallengeSolver(IWebDriverFactory webDriverFactory, IChallengeSolverOptions solverOptions) :
            base(solverOptions) {

            if (webDriverFactory is null)
                throw new ArgumentNullException(nameof(webDriverFactory));

            if (solverOptions is null)
                throw new ArgumentNullException(nameof(solverOptions));

            this.webDriverFactory = webDriverFactory;

        }
        public WebDriverChallengeSolver(IWebDriver webDriver, IChallengeSolverOptions solverOptions) :
            base(solverOptions, disposeWebDriver: false) {

            if (webDriver is null)
                throw new ArgumentNullException(nameof(webDriver));

            this.webDriver = webDriver;

        }

        // Protected members

        protected override IWebDriver CreateWebDriver() {

            if (webDriver is object)
                return webDriver;

            return webDriverFactory.Create();

        }

        // Private members

        private readonly IWebDriver webDriver = null;
        private readonly IWebDriverFactory webDriverFactory;

    }

}