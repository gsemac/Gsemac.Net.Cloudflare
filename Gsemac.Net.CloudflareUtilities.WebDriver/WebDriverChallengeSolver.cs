using Gsemac.Net.SeleniumUtilities;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class WebDriverChallengeSolver :
          WebDriverChallengeSolverBase {

        // Public members

        public IWebDriver WebDriver { get; private set; }

        public WebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }
        public WebDriverChallengeSolver(IWebDriver webDriver, WebDriverChallengeSolverOptions options) :
            base(options, disposeWebDriver: false) {

            this.WebDriver = webDriver;

        }

        // Private members

        protected override IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options, Uri uri) {

            if (WebDriver != null)
                return WebDriver;

            return WebDriverUtilities.CreateWebDriver(options, uri);

        }

    }

}