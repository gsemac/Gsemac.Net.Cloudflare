using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class FirefoxWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public FirefoxWebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(Uri uri, WebDriverChallengeSolverOptions options) {

            Info("Creating web driver (Firefox)");

            FirefoxOptions driverOptions = new FirefoxOptions {
                BrowserExecutableLocation = options.BrowserExecutablePath
            };

            FirefoxDriverService driverService = string.IsNullOrEmpty(options.WebDriverExecutablePath) ?
                FirefoxDriverService.CreateDefaultService() :
                FirefoxDriverService.CreateDefaultService(System.IO.Path.GetDirectoryName(options.WebDriverExecutablePath));

            driverService.HideCommandPromptWindow = true;

            // Resize the window to a reasonable resolution so that viewport matches a conventional monitor viewport.

            driverOptions.AddArguments("-width=1024");
            driverOptions.AddArguments("-height=768");

            if (options.Headless)
                driverOptions.AddArgument("--headless");

            FirefoxProfile profile = new FirefoxProfile {
                DeleteAfterUse = true
            };

            if (!string.IsNullOrEmpty(options.UserAgent))
                profile.SetPreference("general.useragent.override", options.UserAgent);

            if (options.Proxy != null) {

                string proxyAbsoluteUri = options.Proxy.GetProxy(uri).AbsoluteUri;

                Proxy proxy = new Proxy {
                    HttpProxy = proxyAbsoluteUri,
                    SslProxy = proxyAbsoluteUri
                };

                driverOptions.Proxy = proxy;

            }

            // This preference disables the "navigator.webdriver" property.

            profile.SetPreference("dom.webdriver.enabled", false);

            driverOptions.Profile = profile;

            IWebDriver driver = new FirefoxDriver(driverService, driverOptions);

            return driver;

        }

    }

}