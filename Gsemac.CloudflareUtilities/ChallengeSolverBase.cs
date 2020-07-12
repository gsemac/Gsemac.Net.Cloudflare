using Gsemac.Logging;
using System;

namespace Gsemac.CloudflareUtilities {

    public abstract class ChallengeSolverBase :
        LoggableBase,
        IChallengeSolver {

        // Public members

        public abstract IChallengeResponse GetChallengeResponse(Uri uri);

    }

}