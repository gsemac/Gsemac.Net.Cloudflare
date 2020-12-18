using Gsemac.Net.WebBrowsers;
using Gsemac.Net.WebDrivers;
using OpenQA.Selenium;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class WebDriverIuamChallengeSolver :
          WebDriverIuamChallengeSolverBase {

        // Public members

        public IWebDriver WebDriver { get; private set; }

        public WebDriverIuamChallengeSolver(IWebBrowserInfo webBrowserInfo, IWebDriverOptions webDriverOptions, IIuamChallengeSolverOptions solverOptions) :
            base(webDriverOptions, solverOptions) {

            this.webBrowserInfo = webBrowserInfo;

        }
        public WebDriverIuamChallengeSolver(IWebDriver webDriver, IIuamChallengeSolverOptions solverOptions) :
            base(new WebDriverOptions(), solverOptions, disposeWebDriver: false) {

            WebDriver = webDriver;

        }

        // Protected members

        protected override IWebDriver CreateWebDriver(IWebDriverOptions webDriverOptions) {

            if (WebDriver != null)
                return WebDriver;

            return WebDrivers.WebDriver.Create(webBrowserInfo, webDriverOptions);

        }

        // Private members

        private readonly IWebBrowserInfo webBrowserInfo;

    }

}