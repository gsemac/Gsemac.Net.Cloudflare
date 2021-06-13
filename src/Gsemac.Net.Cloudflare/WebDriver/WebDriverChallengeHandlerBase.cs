using Gsemac.Net.WebDrivers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace Gsemac.Net.Cloudflare.WebDriver {

    public abstract class WebDriverChallengeHandlerBase :
        ChallengeHandlerBase {

        // Protected members

        protected WebDriverChallengeHandlerBase(bool disposeWebDriver = true) :
            base("Web Driver IUAM Challenge Solver") {

            this.disposeWebDriver = disposeWebDriver;

        }

        protected override IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, CancellationToken cancellationToken) {

            string url = request.RequestUri.AbsoluteUri;
            IWebDriver driver = null;
            IHttpWebResponse response = null;

            try {

                driver = CreateWebDriver();

                OnLog.Info($"Navigating to {url}");

                driver.Navigate().GoToUrl(url);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(request.Timeout));

                OnLog.Info("Waiting for challenge response");

                // The challenge page may reload several times as it tries new challenges. 
                // We don't want the wait condition to think we've solved the challenge while the page is busy reloading, so it's important to also check for the presence of the <html> element.

                if (wait.Until(d => d.IsDocumentComplete() && CloudflareUtilities.GetProtectionType(d.PageSource) != ProtectionType.ImUnderAttack)) {

                    // We have managed to solve the initial "I'm Under Attack" challenge.

                    ProtectionType challengeType = CloudflareUtilities.GetProtectionType(driver.PageSource);

                    if (challengeType == ProtectionType.CaptchaBypass) {

                        // The captcha page ("Attention Required!") was encountered.
                        // This kind of challenge cannot be solved automatically and requires user interaction. 

                        OnLog.Info("Captcha challenge received");

                        if (wait.Until(d => CloudflareUtilities.GetProtectionType(d.PageSource) != ProtectionType.CaptchaBypass)) {

                            OnLog.Info("Captcha response received");

                            response = CreateSuccessfulChallengeResponse(driver);

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

                        response = CreateSuccessfulChallengeResponse(driver);

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

                if (driver is object && disposeWebDriver) {

                    driver.Quit();

                    driver.Dispose();

                }

            }

            if (response is null)
                throw new ChallengeHandlerException(Properties.ExceptionMessages.ChallengeSolverFailed);

            return response;

        }

        protected abstract IWebDriver CreateWebDriver();

        // Private members

        private readonly bool disposeWebDriver = true;

        private IHttpWebResponse CreateSuccessfulChallengeResponse(IWebDriver driver) {

            return new ChallengeHttpWebResponse(new Uri(driver.Url), driver.PageSource) {
                UserAgent = driver.GetUserAgent(),
                Cookies = driver.GetCookies(),
            };

        }

    }

}