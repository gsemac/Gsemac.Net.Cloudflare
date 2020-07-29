using Gsemac.Net.SeleniumUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public abstract class WebDriverChallengeSolverBase :
        ChallengeSolverBase {

        // Public members

        public override IChallengeResponse GetChallengeResponse(Uri uri) {

            using (IWebDriver driver = CreateWebDriver(options, uri)) {

                string url = uri.AbsoluteUri;
                IChallengeResponse challengeResponse = ChallengeResponse.Failed;

                try {

                    Info($"Navigating to {url}");

                    driver.Navigate().GoToUrl(url);

                    WebDriverWait wait = new WebDriverWait(driver, options.Timeout);

                    Info("Waiting for challenge response");

                    // The challenge page may reload several times as it tries new challenges. 
                    // We don't want the wait condition to think we've solved the challenge while the page is busy reloading, so it's important to also check for the presence of the <html> element.

                    if (wait.Until(d => d.FindElements(By.XPath("//html")).Any() && CloudflareUtilities.GetProtectionType(d.PageSource) != ProtectionType.ImUnderAttack)) {

                        // We have managed to solve the initial "I'm Under Attack" challenge.

                        ProtectionType challengeType = CloudflareUtilities.GetProtectionType(driver.PageSource);

                        if (challengeType == ProtectionType.CaptchaBypass) {

                            // The captcha page ("Attention Required!") was encountered.
                            // This kind of challenge cannot be solved automatically and requires user interaction. 

                            Info("Captcha challenge received");

                            if (options.Headless) {

                                Warning("Solving the captcha challenge requires user interaction, which is not possible when the headless option is enabled.");

                            }
                            else if (wait.Until(d => CloudflareUtilities.GetProtectionType(d.PageSource) != ProtectionType.CaptchaBypass)) {

                                Info("Captcha response received");

                                challengeResponse = CreateSuccessfulChallengeResponse(driver);

                            }
                            else {

                                Error("Failed to receive captcha response (timed out)");

                            }

                        }
                        else if (challengeType == ProtectionType.AccessDenied) {

                            Error("The owner of this website has blocked your IP address.");

                        }
                        else {

                            // The challenge was solved successfully.

                            Info("Challenge response received");

                            challengeResponse = CreateSuccessfulChallengeResponse(driver);

                        }

                    }
                    else {

                        Error("Failed to receive challenge response (timed out)");

                    }

                }
                catch (Exception ex) {

                    Error(ex.ToString());

                    throw ex;

                }
                finally {

                    Info("Closing web driver");

                    driver.Quit();

                }

                return challengeResponse;

            }

        }

        // Protected members

        protected WebDriverChallengeSolverBase(WebDriverChallengeSolverOptions options) {

            this.options = options;

        }

        protected abstract IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options, Uri uri);

        // Private members

        private readonly WebDriverChallengeSolverOptions options;

        private IChallengeResponse CreateSuccessfulChallengeResponse(IWebDriver driver) {

            return new ChallengeResponse(WebDriverUtilities.GetUserAgent(driver), WebDriverUtilities.GetCookies(driver));

        }

    }

}