namespace Gsemac.CloudflareUtilities {

    public interface IChallengeSolverOptions {

        string UserAgent { get; set; }
        int Timeout { get; set; }

    }

}