namespace Gsemac.CloudflareUtilities {

    public abstract class ChallengeSolverOptionsBase :
        IChallengeSolverOptions {

        public string UserAgent { get; set; }
        public int Timeout { get; set; } = 30;

    }

}