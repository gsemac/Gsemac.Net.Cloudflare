using Gsemac.Net.WebDriverUtilities;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class FirefoxWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public FirefoxWebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options, Uri uri) {

            Info("Creating web driver (Firefox)");

            return SeleniumUtilities.CreateFirefoxWebDriver(new WebDriverOptions() {
                BrowserExecutablePath = options.BrowserExecutablePath,
                WebDriverExecutablePath = options.WebDriverExecutablePath,
                UserAgent = options.UserAgent,
                Timeout = options.Timeout,
                Headless = options.Headless,
                Proxy = options.Proxy
            }, uri);

        }

    }

}