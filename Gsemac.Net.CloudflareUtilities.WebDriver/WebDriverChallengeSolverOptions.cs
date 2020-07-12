namespace Gsemac.Net.CloudflareUtilities.WebDriver {

    public class WebDriverChallengeSolverOptions :
        ChallengeSolverOptionsBase {

        public bool Headless { get; set; } = true;
        public string WebDriverExecutablePath { get; set; }
        public string BrowserExecutablePath { get; set; }

    }

}