using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gsemac.CloudflareUtilities.WebDriver {

    public abstract class WebDriverChallengeSolverBase :
        ChallengeSolverBase {

        // Public members

        public override IChallengeResponse GetChallengeResponse(string url) {

            using (IWebDriver driver = CreateWebDriver(options)) {

                try {

                    Info($"Navigating to {url}");

                    driver.Navigate().GoToUrl(url);

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(options.Timeout));

                    Info($"Waiting for challenge");

                    if (wait.Until(d => CloudflareUtilities.GetChallengeType(d.PageSource) == ChallengeType.None)) {

                        Info($"Solved challenge successfully");

                        return new ChallengeResponse(GetUserAgent(driver), GetCookies(driver));

                    }
                    else {

                        Error($"Failed to solve challenge (timeout)");

                        return ChallengeResponse.Failed;

                    }

                }
                catch (Exception ex) {

                    Error(ex.ToString());

                    throw ex;

                }
                finally {

                    Info($"Closing web driver");

                    driver.Close();

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