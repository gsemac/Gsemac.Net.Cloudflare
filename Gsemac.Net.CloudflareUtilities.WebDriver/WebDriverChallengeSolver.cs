using OpenQA.Selenium;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class WebDriverChallengeSolver :
          WebDriverChallengeSolverBase {

        // Public members

        public WebDriverChallengeSolver(IWebDriver webDriver, WebDriverChallengeSolverOptions options) :
            base(options) {

            this.webDriver = webDriver;

        }

        // Private members

        protected override IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options, Uri uri) {

            return webDriver;

        }

        // Private members

        private readonly IWebDriver webDriver;

    }

}