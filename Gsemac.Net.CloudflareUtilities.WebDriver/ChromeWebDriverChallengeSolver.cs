using Gsemac.Net.WebDriverUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class ChromeWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public ChromeWebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options, Uri uri) {

            Info("Creating web driver (Chrome)");

            return SeleniumUtilities.CreateChromeWebDriver(new WebDriverOptions() {
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