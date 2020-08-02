using Gsemac.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gsemac.Net.CloudflareUtilities {

    public abstract class ChallengeSolverBase :
        IChallengeSolver {

        // Public members

        public event LogEventHandler Log;

        public abstract IChallengeResponse GetChallengeResponse(Uri uri);

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper(GetType().Name, Log);

    }

}
