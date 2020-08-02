using Gsemac.Net.SeleniumUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public abstract class WebDriverChallengeSolverBase :
        ChallengeSolverBase {

        // Public members

        public override IChallengeResponse GetChallengeResponse(Uri uri) {

            string url = uri.AbsoluteUri;
            IWebDriver driver = null;
            IChallengeResponse challengeResponse = ChallengeResponse.Failed;

            try {

                driver = CreateWebDriver(options, uri);

                OnLog.Info($"Navigating to {url}");

                driver.Navigate().GoToUrl(url);

                WebDriverWait wait = new WebDriverWait(driver, options.Timeout);

                OnLog.Info("Waiting for challenge response");

                // The challenge page may reload several times as it tries new challenges. 
                // We don't want the wait condition to think we've solved the challenge while the page is busy reloading, so it's important to also check for the presence of the <html> element.

                if (wait.Until(d => d.PageSource.Contains(@"</html>") && CloudflareUtilities.GetProtectionType(d.PageSource) != ProtectionType.ImUnderAttack)) {

                    // We have managed to solve the initial "I'm Under Attack" challenge.

                    ProtectionType challengeType = CloudflareUtilities.GetProtectionType(driver.PageSource);

                    if (challengeType == ProtectionType.CaptchaBypass) {

                        // The captcha page ("Attention Required!") was encountered.
                        // This kind of challenge cannot be solved automatically and requires user interaction. 

                        OnLog.Info("Captcha challenge received");

                        if (options.Headless) {

                            OnLog.Warning("Solving the captcha challenge requires user interaction, which is not possible when the headless option is enabled.");

                        }
                        else if (wait.Until(d => CloudflareUtilities.GetProtectionType(d.PageSource) != ProtectionType.CaptchaBypass)) {

                            OnLog.Info("Captcha response received");

                            challengeResponse = CreateSuccessfulChallengeResponse(driver);

                        }
                        else {

                            OnLog.Error("Failed to receive captcha response (timed out)");

                        }

                    }
                    else if (challengeType == ProtectionType.AccessDenied) {

                        OnLog.Error("The owner of this website has blocked your IP address.");

                    }
                    else {

                        // The challenge was solved successfully.

                        OnLog.Info("Challenge response received");

                        challengeResponse = CreateSuccessfulChallengeResponse(driver);

                    }

                }
                else {

                    OnLog.Error("Failed to receive challenge response (timed out)");

                }

            }
            catch (Exception ex) {

                OnLog.Error(ex.ToString());

                throw ex;

            }
            finally {

                OnLog.Info("Closing web driver");

                if (driver != null && disposeWebDriver) {

                    driver.Quit();

                    driver.Dispose();

                }

            }

            return challengeResponse;

        }

        // Protected members

        protected WebDriverChallengeSolverBase(WebDriverChallengeSolverOptions options, bool disposeWebDriver = true) :
            base("CF Challenge Solver (Web Driver)") {

            this.options = options;
            this.disposeWebDriver = disposeWebDriver;

        }

        protected abstract IWebDriver CreateWebDriver(WebDriverChallengeSolverOptions options, Uri uri);

        // Private members

        private readonly bool disposeWebDriver = true;
        private readonly WebDriverChallengeSolverOptions options;

        private IChallengeResponse CreateSuccessfulChallengeResponse(IWebDriver driver) {

            return new ChallengeResponse(WebDriverUtilities.GetUserAgent(driver), WebDriverUtilities.GetCookies(driver));

        }

    }

}