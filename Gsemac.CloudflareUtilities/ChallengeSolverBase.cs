using Gsemac.Logging;

namespace Gsemac.CloudflareUtilities {

    public abstract class ChallengeSolverBase :
        LoggableBase,
        IChallengeSolver {

        // Public members

        public abstract IChallengeResponse GetChallengeResponse(string url);

    }

}