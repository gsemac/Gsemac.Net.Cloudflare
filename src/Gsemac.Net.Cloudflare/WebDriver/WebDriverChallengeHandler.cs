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
        public WebDriverChallengeHandler(IWebDriverFactory webDriverFactory, ILogger logger) :
            base(disposeWebDriver: true, logger) {

            if (webDriverFactory is null)
                throw new ArgumentNullException(nameof(webDriverFactory));

            this.webDriverFactory = webDriverFactory;

        }
        public WebDriverChallengeHandler(IWebDriver webDriver) :
            this(webDriver, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IWebDriver webDriver, ILogger logger) :
            base(disposeWebDriver: false, logger) {

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