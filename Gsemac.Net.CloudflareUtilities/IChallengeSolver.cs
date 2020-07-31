using Gsemac.Logging;
using System;

namespace Gsemac.Net.CloudflareUtilities {

    public interface IChallengeSolver :
        ILoggable,
        IDisposable {

        IChallengeResponse GetChallengeResponse(Uri uri);

    }

}