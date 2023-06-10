using Gsemac.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Gsemac.Net.Cloudflare {

    internal class ChallengeSolutionCache :
        IChallengeSolutionCache {

        // Public members

        public void Add(Uri requestUri, IChallengeSolution solution) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            if (solution is null)
                throw new ArgumentNullException(nameof(solution));

            lock (cacheMutex) {

                // All cookies in the solution will have been retrieved using a single user agent, so we can associate the same one with all cookies.

                foreach (Cookie cookie in solution.Cookies) {

                    userAgentCache[cookie.Domain] = solution.UserAgent;

                    // FlareSolverr has occassionally returned cookies with invalid characters (such as commas).
                    // Avoid adding any of these cookies to the cookie container, because it will throw an exception.

                    if (cookie.Value.Any(c => HttpUtilities.GetInvalidCookieChars().Contains(c)))
                        return;

                    // Don't bother caching cookies that are unrelated to Cloudflare.
                    // Cloudflare cookies begin with "cf_", "_cf", or "__cf".
                    // https://developers.cloudflare.com/fundamentals/get-started/reference/cloudflare-cookies/
                    // This offers the user more control over things like (unrelated) session cookies that may have also been set.

                    if (IsCloudflareCookie(cookie))
                        cookieCache.Add(cookie);

                }

            }

        }
        public bool TryGet(Uri requestUri, out IChallengeSolution solution) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            solution = null;

            lock (cacheMutex) {

                CookieCollection clearanceCookies = cookieCache.GetCookies(requestUri);

                if (clearanceCookies.Count > 0) {

                    string userAgent = userAgentCache[clearanceCookies[0].Domain];

                    solution = new ChallengeSolution(userAgent, clearanceCookies);

                    return true;

                }

            }

            return solution is object;

        }

        // Private members

        private readonly object cacheMutex = new object();
        private readonly IDictionary<string, string> userAgentCache = new Dictionary<string, string>();
        private readonly CookieContainer cookieCache = new CookieContainer();

        private static bool IsCloudflareCookie(Cookie cookie) {

            if (cookie is null)
                throw new ArgumentNullException(nameof(cookie));

            return cookie.Name.StartsWith("cf_") ||
                cookie.Name.StartsWith("_cf") ||
                cookie.Name.StartsWith("__cf");

        }

    }

}