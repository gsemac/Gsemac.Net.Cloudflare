using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;
using System.Drawing;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class FirefoxWebDriverIuamChallengeSolver :
        WebDriverIuamChallengeSolverBase {

        // Public members

        public FirefoxWebDriverIuamChallengeSolver(IWebDriverIuamChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(IWebDriverIuamChallengeSolverOptions options, Uri uri) {

            OnLog.Info("Creating web driver (Firefox)");

            return WebDriverUtilities.CreateFirefoxWebDriver(new WebDriverOptions() {
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