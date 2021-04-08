using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IIuamChallengeSolver :
        ILogEventSource {

        string Name { get; }

        IIuamChallengeResponse GetChallengeResponse(Uri uri);

    }

}