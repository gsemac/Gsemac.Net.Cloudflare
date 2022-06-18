using Gsemac.Net.Http;
using System;
using System.Collections.Generic;

namespace Gsemac.Net.Cloudflare {

    internal class ChallengeSolutionCache :
        IChallengeSolutionCache {

        // Public members

        public void Add(Uri requestUri, IChallengeSolution solution) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            if (solution is null)
                throw new ArgumentNullException(nameof(solution));

            string key = $".{Url.GetHostname(requestUri.AbsoluteUri)}";

            lock (cachedSolutions)
                cachedSolutions[key] = solution;

        }
        public IChallengeSolution Get(Uri requestUri) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            lock (cachedSolutions) {

                // We want subdomains to be able to use the same cookies as the primary domains, as would occur in a web browser.

                foreach (string key in cachedSolutions.Keys) {

                    if (new CookieDomainPattern(key).IsMatch(requestUri))
                        return cachedSolutions[key];

                }

                return null;

            }

        }

        // Private members

        private readonly IDictionary<string, IChallengeSolution> cachedSolutions = new Dictionary<string, IChallengeSolution>();

    }

}