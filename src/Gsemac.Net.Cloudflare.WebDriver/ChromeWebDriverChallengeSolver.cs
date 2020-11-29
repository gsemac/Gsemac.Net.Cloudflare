using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;
using System.Drawing;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class ChromeWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public ChromeWebDriverChallengeSolver(IWebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(IWebDriverChallengeSolverOptions options, Uri uri) {

            OnLog.Info("Creating web driver (Chrome)");

            return WebDriverUtilities.CreateChromeWebDriver(new WebDriverOptions() {
                BrowserExecutablePath = options.BrowserExecutablePath,
                WebDriverExecutablePath = options.WebDriverExecutablePath ?? "chromedriver.exe",
                UserAgent = options.UserAgent,
                Timeout = options.Timeout,
                Headless = options.Headless,
                Proxy = options.Proxy,
                WindowSize = new Size(1024, 768)
            }, uri);

        }

    }

}