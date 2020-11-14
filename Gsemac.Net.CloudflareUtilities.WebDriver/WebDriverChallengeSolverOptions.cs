using Gsemac.Net.WebDrivers;

namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class WebDriverChallengeSolverOptions :
        WebDriverOptions,
        IWebDriverChallengeSolverOptions {

        public WebDriverChallengeSolverOptions() { }
        public WebDriverChallengeSolverOptions(IWebDriverOptions options) {

            this.Proxy = options.Proxy;
            this.Timeout = options.Timeout;
            this.UserAgent = options.UserAgent;
            this.WebDriverExecutablePath = options.WebDriverExecutablePath;
            this.BrowserExecutablePath = options.BrowserExecutablePath;
            this.Headless = options.Headless;
            this.WindowPosition = options.WindowPosition;
            this.WindowSize = options.WindowSize;

        }

    }

}