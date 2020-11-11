using Gsemac.Net.WebDriverUtilities;
using OpenQA.Selenium;
using System;
using System.Drawing;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class FirefoxWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public FirefoxWebDriverChallengeSolver(IWebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(IWebDriverChallengeSolverOptions options, Uri uri) {

            OnLog.Info("Creating web driver (Firefox)");

            return WebDriverUtilities.WebDriverUtilities.CreateFirefoxWebDriver(new WebDriverOptions() {
                BrowserExecutablePath = options.BrowserExecutablePath,
                WebDriverExecutablePath = options.WebDriverExecutablePath ?? "geckodriver.exe",
                UserAgent = options.UserAgent,
                Timeout = options.Timeout,
                Headless = options.Headless,
                Proxy = options.Proxy,
                WindowSize = new Size(1024, 768)
            }, uri);

        }

    }

}