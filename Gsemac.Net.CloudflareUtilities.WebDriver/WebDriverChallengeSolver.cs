using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class WebDriverChallengeSolver :
          WebDriverChallengeSolverBase {

        // Public members

        public IWebDriver WebDriver { get; private set; }

        public WebDriverChallengeSolver(IWebDriverChallengeSolverOptions options) :
            base(options) {
        }
        public WebDriverChallengeSolver(IWebDriver webDriver, IWebDriverChallengeSolverOptions options) :
            base(options, disposeWebDriver: false) {

            this.WebDriver = webDriver;

        }

        // Private members

        protected override IWebDriver CreateWebDriver(IWebDriverChallengeSolverOptions options, Uri uri) {

            if (WebDriver != null)
                return WebDriver;

            return WebDriverUtilities.CreateWebDriver(options, uri);

        }

    }

}