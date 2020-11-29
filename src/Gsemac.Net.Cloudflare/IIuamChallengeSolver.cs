using Gsemac.Logging;
using System;

namespace Gsemac.Net.Cloudflare {

    public interface IIuamChallengeSolver :
        ILoggable {

        IIuamChallengeResponse GetChallengeResponse(Uri uri);

    }

}