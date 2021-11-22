using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare {

    public abstract class ChallengeHandlerBase :
        HttpWebRequestHandler {

        // Public members

        public string Name { get; }

        // Protected members

        protected ChallengeHandlerBase() :
            this(nameof(ChallengeHandlerBase)) {
        }
        protected ChallengeHandlerBase(string name) :
            this(name, ChallengeHandlerOptions.Default) {
        }
        protected ChallengeHandlerBase(IChallengeHandlerOptions options) :
         this(nameof(ChallengeHandlerBase), options) {
        }
        protected ChallengeHandlerBase(string name, IChallengeHandlerOptions options) {

            Name = name;
            this.options = options;

        }

        protected sealed override IHttpWebResponse Send(IHttpWebRequest request, CancellationToken cancellationToken) {

            try {

                // Set the cookies and user agent on the request to the ones we've cached.

                if (options.RememberCookies)
                    ApplySolutionToRequest(request, GetCachedSolution(request.RequestUri));

                return base.Send(request, cancellationToken);

            }
            catch (WebException webEx) {

                // Even though 1020 "Access Denied" errors can't be "solved", they're sometimes the result of Cloudflare detecting something unusual about the request (e.g. header order).
                // It's worth letting the solver make an attempt in case it's able to have the request go through successfully.

                bool isCloudflareDetected = webEx.Response is object && CloudflareUtilities.IsProtectionDetected(webEx.Response);

                if (!isCloudflareDetected)
                    throw;

                // We've encountered a challenge, so we need to pass resposibility off to the derived class.
                // We still want delegating handlers nested further down to be able to see the request before we get a response, so we'll create a new request that returns the solver's response.

                IHttpWebRequest solverRequest = new ChallengeHandlerHttpWebRequest(request, r => GetChallengeResponse(r, cancellationToken));

                bool solverThrewAnException = false;

                try {

                    IHttpWebResponse response = base.Send(solverRequest, cancellationToken);

                    // The resulting HttpWebResponse should be a ChallengeHttpWebResponse unless the derived class decided to return something else.
                    // #todo This should really be its own interface accessed through the HandlerHttpWebRequest object to avoid having to downcast.

                    if (response is ChallengeHandlerHttpWebResponse challengeResponse) {

                        // Cache the solution if applicable so that it can be sent with future requests.

                        IChallengeSolution solution = challengeResponse.Solution;

                        if (options.RememberCookies)
                            SetCachedSolution(response.ResponseUri, solution);

                        if (!challengeResponse.HasResponseStream) {

                            // Some solvers may not return a response body, so we want to be able to retry the request using the cookies/user agent provided by the solver.

                            response.Close();

                            ApplySolutionToRequest(request, solution);

                            return base.Send(request, cancellationToken);

                        }
                        else {

                            // Simply return the response, because it already has a response body.

                            return response;

                        }

                    }
                    else {

                        // The response is not a ChallengeHandlerHttpWebResponse instance, so we can't cache the solution.

                        return response;

                    }

                }
                catch (Exception challengeHandlerEx) {

                    solverThrewAnException = true;

                    // If the challenge solver throws an exception, we still want the original response so the caller can read it.

                    throw new WebException(Properties.ExceptionMessages.ChallengeSolverThrewAnException, challengeHandlerEx, webEx.Status, webEx.Response);

                }
                finally {

                    // Make sure to close the response before returning, because it will not be accessible to the caller.
                    // We'll only do this if the solver didn't throw an exception, because otherwise the response is provided in the wrapping WebException.

                    if (!solverThrewAnException && webEx.Response is object)
                        webEx.Response.Close();

                }

            }

        }
        protected abstract IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, CancellationToken cancellationToken);

        // Private members

        private readonly IChallengeHandlerOptions options = new ChallengeHandlerOptions();
        private readonly IDictionary<string, IChallengeSolution> cachedSolutions = new Dictionary<string, IChallengeSolution>();

        private IChallengeSolution GetCachedSolution(Uri requestUri) {

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
        private void SetCachedSolution(Uri requestUri, IChallengeSolution solution) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            if (solution is null)
                throw new ArgumentNullException(nameof(solution));

            string key = $".{Url.GetHostname(requestUri.AbsoluteUri)}";

            lock (cachedSolutions)
                cachedSolutions[key] = solution;

        }

        private static void ApplySolutionToRequest(IHttpWebRequest request, IChallengeSolution solution) {

            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (solution is object) {

                if (!string.IsNullOrWhiteSpace(solution.UserAgent))
                    request.UserAgent = solution.UserAgent;

                request.CookieContainer.Add(solution.Cookies);

            }

        }

    }

}