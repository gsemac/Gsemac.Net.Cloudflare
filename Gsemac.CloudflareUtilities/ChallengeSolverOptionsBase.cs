using System;
using System.Collections.Generic;
using System.Text;

namespace Gsemac.CloudflareUtilities {

    public abstract class ChallengeSolverOptionsBase :
        IChallengeSolverOptions {

        public string UserAgent { get; set; }

    }

}