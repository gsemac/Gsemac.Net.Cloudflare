using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare {

    public interface IChallengeSolver :
        ILogEventSource {

        string Name { get; }

        IChallengeResponse GetResponse(Uri uri);

    }

}