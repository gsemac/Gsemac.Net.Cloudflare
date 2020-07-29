using Gsemac.Net.SeleniumUtilities;
using OpenQA.Selenium;
using System;
using System.Drawing;

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

            return WebDriverUtilities.CreateFirefoxWebDriver(new WebDriverOptions() {
                BrowserExecutablePath = options.BrowserExecutablePath,
                WebDriverExecutablePath = options.WebDriverExecutablePath,
                UserAgent = options.UserAgent,
                Timeout = options.Timeout,
                Headless = options.Headless,
                Proxy = options.Proxy,
                WindowSize = new Size(1024, 768)
            }, uri);

        }

    }

}