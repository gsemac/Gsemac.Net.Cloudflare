using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public abstract class WebDriverChallengeSolverBase :
        ChallengeSolverBase {

        // Public members

        public override IChallengeResponse GetChallengeResponse(Uri uri) {

            using (IWebDriver driver = CreateWebDriver(uri, options)) {

                string url = uri.AbsoluteUri;
                IChallengeResponse challengeResponse = ChallengeResponse.Failed;

                try {

                    Info($"Navigating to {url}");

                    driver.Navigate().GoToUrl(url);

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(options.Timeout));

                    Info($"Waiting for challenge response");

                    if (wait.Until(d => CloudflareUtilities.GetChallengeType(d.PageSource) != ChallengeType.ImUnderAttack)) {

                        // We have managed to solve the initial "I'm Under Attack" challenge.

                        ChallengeType challengeType = CloudflareUtilities.GetChallengeType(driver.PageSource);

                        if (challengeType == ChallengeType.CaptchaBypass) {

                            // The captcha page ("Attention Required!") was encountered.
                            // This kind of challenge cannot be solved automatically and requires user interaction. 

                            Info($"Captcha challenge received");

                            if (options.Headless) {

                                Warning($"Solving the captcha challenge requires user interaction, which is not possible when the headless option is enabled.");

                            }
                            else if (wait.Until(d => CloudflareUtilities.GetChallengeType(d.PageSource) != ChallengeType.CaptchaBypass)) {

                                Info($"Captcha response received");

                                challengeResponse = CreateSuccessfulChallengeResponse(driver);

                            }
                            else {

                                Error($"Failed to receive captcha response (timed out)");

                            }

                        }
                        else {

                            // The challenge was solved successfully.

                            Info($"Challenge response received");

                            challengeResponse = CreateSuccessfulChallengeResponse(driver);

                        }

                    }
                    else {

                        Error($"Failed to receive challenge response (timed out)");

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

                return challengeResponse;

            }

        }

        // Protected members

        protected WebDriverChallengeSolverBase(WebDriverChallengeSolverOptions options) {

            this.options = options;

        }

        protected abstract IWebDriver CreateWebDriver(Uri uri, WebDriverChallengeSolverOptions options);

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
        private IChallengeResponse CreateSuccessfulChallengeResponse(IWebDriver driver) {

            return new ChallengeResponse(GetUserAgent(driver), GetCookies(driver));

        }

    }

}