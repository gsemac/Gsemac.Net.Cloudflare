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

        protected override IWebDriver CreateWebDriver(Uri uri, WebDriverChallengeSolverOptions options) {

            Info("Creating web driver (Chrome)");

            ChromeOptions driverOptions = new ChromeOptions {
                BinaryLocation = options.BrowserExecutablePath
            };

            ChromeDriverService driverService = string.IsNullOrEmpty(options.WebDriverExecutablePath) ?
                ChromeDriverService.CreateDefaultService() :
                ChromeDriverService.CreateDefaultService(System.IO.Path.GetDirectoryName(options.WebDriverExecutablePath));

            driverService.HideCommandPromptWindow = true;

            // Resize the window to a reasonable resolution so that viewport matches a conventional monitor viewport.

            driverOptions.AddArgument("--window-size=1024,768");

            if (options.Headless)
                driverOptions.AddArgument("--headless");

            if (!string.IsNullOrEmpty(options.UserAgent))
                driverOptions.AddArgument($"--user-agent={options.UserAgent}");

            if (options.Proxy != null)
                driverOptions.AddArgument($"--proxy-server={options.Proxy.GetProxy(uri).AbsoluteUri}");

            IWebDriver driver = new ChromeDriver(driverService, driverOptions);

            return driver;

        }

    }

}