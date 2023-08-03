using Gsemac.IO.Logging;
using Gsemac.Net.Http;
using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;
using System;

namespace Gsemac.Net.Cloudflare.Selenium {

    public class WebDriverChallengeHandler :
          WebDriverChallengeHandlerBase {

        // Public members

        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriverFactory webDriverFactory) :
            this(webRequestFactory, webDriverFactory, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriverFactory webDriverFactory, ILogger logger) :
            this(webRequestFactory, webDriverFactory, ChallengeHandlerOptions.Default, logger) {
        }
        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriverFactory webDriverFactory, IChallengeHandlerOptions options) :
            this(webRequestFactory, webDriverFactory, options, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriverFactory webDriverFactory, IChallengeHandlerOptions options, ILogger logger) :
            base(webRequestFactory, disposeWebDriver: true, options, logger) {

            if (webDriverFactory is null)
                throw new ArgumentNullException(nameof(webDriverFactory));

            this.webDriverFactory = webDriverFactory;


        }
        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriver webDriver) :
            this(webRequestFactory, webDriver, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriver webDriver, ILogger logger) :
            this(webRequestFactory, webDriver, ChallengeHandlerOptions.Default, logger) {
        }
        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriver webDriver, IChallengeHandlerOptions options) :
            this(webRequestFactory, webDriver, options, new NullLogger()) {
        }
        public WebDriverChallengeHandler(IHttpWebRequestFactory webRequestFactory, IWebDriver webDriver, IChallengeHandlerOptions options, ILogger logger) :
            base(webRequestFactory, disposeWebDriver: false, options, logger) {

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