using Gsemac.IO.Logging;
using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.Cloudflare.WebDriver {

    public class WebDriverChallengeHandler :
          WebDriverChallengeHandlerBase {

        // Public members

        public WebDriverChallengeHandler(IWebDriverFactory webDriverFactory) :
            this(webDriverFactory, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IWebDriverFactory webDriverFactory, IChallengeHandlerOptions options) :
            this(webDriverFactory, options, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IWebDriverFactory webDriverFactory, ILogger logger) :
            this(webDriverFactory, ChallengeHandlerOptions.Default, logger) {
        }
        public WebDriverChallengeHandler(IWebDriverFactory webDriverFactory, IChallengeHandlerOptions options, ILogger logger) :
            base(disposeWebDriver: true, options, logger) {

            if (webDriverFactory is null)
                throw new ArgumentNullException(nameof(webDriverFactory));

            this.webDriverFactory = webDriverFactory;


        }
        public WebDriverChallengeHandler(IWebDriver webDriver) :
            this(webDriver, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IWebDriver webDriver, IChallengeHandlerOptions options) :
            this(webDriver, options, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IWebDriver webDriver, ILogger logger) :
            this(webDriver, ChallengeHandlerOptions.Default, logger) {
        }
        public WebDriverChallengeHandler(IWebDriver webDriver, IChallengeHandlerOptions options, ILogger logger) :
            base(disposeWebDriver: false, options, logger) {

            if (webDriver is null)
                throw new ArgumentNullException(nameof(webDriver));

            this.webDriver = webDriver;

        }

        // Protected members

        protected override IWebDriver CreateWebDriver() {

            if (webDriver is object)
                return webDriver;

            return webDriverFactory.Create();

        }

        // Private members

        private readonly IWebDriver webDriver = null;
        private readonly IWebDriverFactory webDriverFactory;

    }

}