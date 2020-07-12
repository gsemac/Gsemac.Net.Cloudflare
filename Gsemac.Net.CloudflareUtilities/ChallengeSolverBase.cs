using Gsemac.Logging;
using System;

namespace Gsemac.Net.CloudflareUtilities {

    public abstract class ChallengeSolverBase :
        LoggableBase,
        IChallengeSolver {

        // Public members

        public abstract IChallengeResponse GetChallengeResponse(Uri uri);

    }

}