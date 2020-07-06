using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gsemac.CloudflareUtilities {

    public abstract class WebDriverChallengeSolverBase :
        IChallengeSolver {

        // Public members

        public IChallengeResponse GetChallengeResponse(string url) {

            using (IWebDriver driver = CreateWebDriver(options)) {

                driver.Navigate().GoToUrl(url);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(options.Timeout));

                if (wait.Until(d => !d.FindElements(By.XPath("//div[contains(@class, 'cf-browser-verification')]")).Any())) {

                    return new ChallengeResponse(GetUserAgent(driver), GetCookies(driver));

                }
                else {

                    return ChallengeResponse.Failed;

                }

            }

        }

        // Protected members

        protected WebDriverChallengeSolverBase(WebDriverChallengeSolverOptions options) {

            this.options = options;

        }

        protected abstract IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options);

        // Private members

        private readonly WebDriverChallengeSolverOptions options;

        private string GetUserAgent(IWebDriver driver) {

            IJavaScriptExecutor javascriptExecutor = (IJavaScriptExecutor)driver;

            string userAgent = (string)javascriptExecutor.ExecuteScript("return navigator.userAgent");

            return userAgent;

        }
        private IDictionary<string, string> GetCookies(IWebDriver driver) {

            return driver.Manage().Cookies.AllCookies.ToDictionary(cookie => cookie.Name, cookie => cookie.Value);

        }

    }

}