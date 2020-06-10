using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gsemac.CloudflareUtilities {

    public class SeleniumChallengeSolver :
        IChallengeSolver {

        // Public members

        public SeleniumChallengeSolver(WebDriverChallengeSolverOptions options) {

            this.options = options;

        }

        public IChallengeResponse GetChallengeResponse(string url) {

            using (IWebDriver driver = CreateWebDriver()) {

                driver.Navigate().GoToUrl(url);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

                if (wait.Until(d => !d.FindElements(By.XPath("//div[contains(@class, 'cf-browser-verification')]")).Any())) {

                    IJavaScriptExecutor javascriptExecutor = (IJavaScriptExecutor)driver;

                    string userAgent = (string)javascriptExecutor.ExecuteScript("return navigator.userAgent");
                    IDictionary<string, string> cookies = driver.Manage().Cookies.AllCookies.ToDictionary(cookie => cookie.Name, cookie => cookie.Value);

                    return new ChallengeResponse(true, userAgent, cookies);

                }
                else {

                    return ChallengeResponse.Failed;

                }

            }

        }

        // Private members

        private readonly WebDriverChallengeSolverOptions options;

        private IWebDriver CreateWebDriver() {

            string browserExecutableFilename = System.IO.Path.GetFileName(options.BrowserExecutablePath);

            switch (browserExecutableFilename.ToLowerInvariant()) {

                default:
                    return CreateFirefoxWebDriver();

            }

        }
        private IWebDriver CreateFirefoxWebDriver() {

            FirefoxOptions driverOptions = new FirefoxOptions {
                BrowserExecutableLocation = options.BrowserExecutablePath
            };

            if (options.Headless)
                driverOptions.AddArgument("--headless");

            FirefoxProfile profile = new FirefoxProfile {
                DeleteAfterUse = true
            };

            // This preference disables the "navigator.webdriver" property.

            profile.SetPreference("dom.webdriver.enabled", false);

            driverOptions.Profile = profile;

            IWebDriver driver = new FirefoxDriver(driverOptions);

            return driver;

        }

    }

}