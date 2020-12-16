﻿using Gsemac.Net.WebDrivers.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Gsemac.Net.Cloudflare.Iuam {

    public abstract class WebDriverIuamChallengeSolverBase :
        IuamChallengeSolverBase {

        // Public members

        public override IIuamChallengeResponse GetChallengeResponse(Uri uri) {

            string url = uri.AbsoluteUri;
            IWebDriver driver = null;
            IIuamChallengeResponse challengeResponse = IuamChallengeResponse.Failed;

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

        protected WebDriverIuamChallengeSolverBase(IWebDriverIuamChallengeSolverOptions options, bool disposeWebDriver = true) :
            base("Web Driver IUAM Challenge Solver") {

            this.options = options;
            this.disposeWebDriver = disposeWebDriver;

        }

        protected abstract IWebDriver CreateWebDriver(IWebDriverIuamChallengeSolverOptions options, Uri uri);

        // Private members

        private readonly bool disposeWebDriver = true;
        private readonly IWebDriverIuamChallengeSolverOptions options;

        private IIuamChallengeResponse CreateSuccessfulChallengeResponse(IWebDriver driver) {

            return new IuamChallengeResponse(driver.GetUserAgent(), driver.GetCookies());

        }

    }

}