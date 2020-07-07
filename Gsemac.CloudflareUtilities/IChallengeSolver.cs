using Gsemac.Logging;
using System;

namespace Gsemac.CloudflareUtilities {

    public interface IChallengeSolver :
        ILoggable {

        IChallengeResponse GetChallengeResponse(string url);

    }

}