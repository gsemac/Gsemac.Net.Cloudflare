using Gsemac.Net.SeleniumUtilities;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class WebDriverChallengeSolver :
          WebDriverChallengeSolverBase {

        // Public members

        public WebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }
        public WebDriverChallengeSolver(IWebDriver webDriver, WebDriverChallengeSolverOptions options) :
            base(options, disposeWebDriver: false) {

            this.webDriver = webDriver;

        }

        // Private members

        protected override IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options, Uri uri) {

            if (webDriver != null)
                return webDriver;

            return WebDriverUtilities.CreateWebDriver(options, uri);

        }

        // Private members

        private readonly IWebDriver webDriver;

    }

}