using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Gsemac.CloudflareUtilities.WebDriver {

    public class FirefoxWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public FirefoxWebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options) {

            Info("Creating web driver (Firefox)");

            FirefoxOptions driverOptions = new FirefoxOptions {
                BrowserExecutableLocation = options.BrowserExecutablePath
            };

            if (options.Headless)
                driverOptions.AddArgument("--headless");

            FirefoxProfile profile = new FirefoxProfile {
                DeleteAfterUse = true
            };

            if (!string.IsNullOrEmpty(options.UserAgent))
                profile.SetPreference("general.useragent.override", options.UserAgent);

            // This preference disables the "navigator.webdriver" property.

            profile.SetPreference("dom.webdriver.enabled", false);

            driverOptions.Profile = profile;

            IWebDriver driver = new FirefoxDriver(driverOptions);

            return driver;

        }

    }

}