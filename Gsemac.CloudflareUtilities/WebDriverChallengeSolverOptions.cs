namespace Gsemac.CloudflareUtilities {

    public class WebDriverChallengeSolverOptions :
        ChallengeSolverOptionsBase {

        public bool Headless { get; set; } = true;
        public string WebDriverExecutablePath { get; set; }
        public string BrowserExecutablePath { get; set; }

    }

}