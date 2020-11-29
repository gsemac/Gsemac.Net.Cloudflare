using Gsemac.Net.WebDrivers;

namespace Gsemac.Net.Cloudflare.WebDrivers {

    public class WebDriverIuamChallengeSolverOptions :
        WebDriverOptions,
        IWebDriverIuamChallengeSolverOptions {

        public WebDriverIuamChallengeSolverOptions() { }
        public WebDriverIuamChallengeSolverOptions(IWebDriverOptions options) {

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