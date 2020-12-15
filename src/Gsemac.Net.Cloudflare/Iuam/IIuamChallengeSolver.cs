using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IIuamChallengeSolver :
        ILoggable {

        IIuamChallengeResponse GetChallengeResponse(Uri uri);

    }

}