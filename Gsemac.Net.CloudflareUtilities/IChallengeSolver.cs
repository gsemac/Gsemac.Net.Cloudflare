using Gsemac.Logging;
using System;

namespace Gsemac.Net.CloudflareUtilities {

    public interface IChallengeSolver :
        ILoggable {

        IChallengeResponse GetChallengeResponse(Uri uri);

    }

}