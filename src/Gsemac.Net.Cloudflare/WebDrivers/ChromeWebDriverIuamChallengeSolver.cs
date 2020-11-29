using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;
using System.Drawing;

namespace Gsemac.Net.Cloudflare.WebDrivers {

    public class ChromeWebDriverIuamChallengeSolver :
        WebDriverIuamChallengeSolverBase {

        // Public members

        public ChromeWebDriverIuamChallengeSolver(IWebDriverIuamChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(IWebDriverIuamChallengeSolverOptions options, Uri uri) {

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