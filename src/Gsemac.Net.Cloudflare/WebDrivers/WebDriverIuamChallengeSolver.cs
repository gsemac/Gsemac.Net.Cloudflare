using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.Cloudflare.WebDrivers {

    public class WebDriverIuamChallengeSolver :
          WebDriverIuamChallengeSolverBase {

        // Public members

        public IWebDriver WebDriver { get; private set; }

        public WebDriverIuamChallengeSolver(IWebDriverIuamChallengeSolverOptions options) :
            base(options) {
        }
        public WebDriverIuamChallengeSolver(IWebDriver webDriver, IWebDriverIuamChallengeSolverOptions options) :
            base(options, disposeWebDriver: false) {

            this.WebDriver = webDriver;

        }

        // Private members

        protected override IWebDriver CreateWebDriver(IWebDriverIuamChallengeSolverOptions options, Uri uri) {

            if (WebDriver != null)
                return WebDriver;

            return WebDriverUtilities.CreateWebDriver(options, uri);

        }

    }

}