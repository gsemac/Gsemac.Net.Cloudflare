using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace Gsemac.CloudflareUtilities.WebDriver {

    public class ChromeWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public ChromeWebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(Uri uri, WebDriverChallengeSolverOptions options) {

            Info("Creating web driver (Chrome)");

            ChromeOptions driverOptions = new ChromeOptions {
                BinaryLocation = options.BrowserExecutablePath
            };

            if (options.Headless)
                driverOptions.AddArgument("--headless");

            if (!string.IsNullOrEmpty(options.UserAgent))
                driverOptions.AddArgument($"--user-agent={options.UserAgent}");

            if (options.Proxy != null)
                driverOptions.AddArgument($"--proxy-server={options.Proxy.GetProxy(uri).AbsoluteUri}");

            IWebDriver driver = new ChromeDriver(driverOptions);

            return driver;

        }

    }

}