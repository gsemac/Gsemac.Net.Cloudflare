using System;

namespace Gsemac.Net.Cloudflare {

    internal interface IChallengeSolutionCache {

        IChallengeSolution Get(Uri requestUri);
        void Add(Uri requestUri, IChallengeSolution solution);

    }

}