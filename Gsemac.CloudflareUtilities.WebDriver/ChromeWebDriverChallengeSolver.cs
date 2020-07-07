using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Gsemac.CloudflareUtilities.WebDriver {

    public class ChromeWebDriverChallengeSolver :
        WebDriverChallengeSolverBase {

        // Public members

        public ChromeWebDriverChallengeSolver(WebDriverChallengeSolverOptions options) :
            base(options) {
        }

        // Protected members

        protected override IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options) {

            ChromeOptions driverOptions = new ChromeOptions {
                BinaryLocation = options.BrowserExecutablePath
            };

            if (options.Headless)
                driverOptions.AddArgument("--headless");

            if (!string.IsNullOrEmpty(options.UserAgent))
                driverOptions.AddArgument($"user-agent={options.UserAgent}");

            IWebDriver driver = new ChromeDriver(driverOptions);

            return driver;

        }

    }

}