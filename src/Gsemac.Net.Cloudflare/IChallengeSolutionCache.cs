using System;

namespace Gsemac.Net.Cloudflare {

    internal interface IChallengeSolutionCache {

        void Add(Uri requestUri, IChallengeSolution solution);

       bool TryGet(Uri requestUri, out IChallengeSolution solution);

    }

}