using Gsemac.Net.WebDrivers;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class WebDriverIuamChallengeSolverOptions :
        WebDriverOptions,
        IWebDriverIuamChallengeSolverOptions {

        public WebDriverIuamChallengeSolverOptions() { }
        public WebDriverIuamChallengeSolverOptions(IWebDriverOptions options) {

            Proxy = options.Proxy;
            Timeout = options.Timeout;
            UserAgent = options.UserAgent;
            WebDriverExecutablePath = options.WebDriverExecutablePath;
            BrowserExecutablePath = options.BrowserExecutablePath;
            Headless = options.Headless;
            WindowPosition = options.WindowPosition;
            WindowSize = options.WindowSize;

        }

    }

}