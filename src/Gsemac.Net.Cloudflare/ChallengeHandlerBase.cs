using Gsemac.Net.Cloudflare.Properties;
using Gsemac.Net.Extensions;
using Gsemac.Net.Http;
using Gsemac.Reflection;
using System;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare {

    public abstract class ChallengeHandlerBase :
        HttpWebRequestHandler {

        // Public members

        public string Name { get; }

        // Protected members

        protected ChallengeHandlerBase(IHttpWebRequestFactory webRequestFactory) :
            this(webRequestFactory, nameof(ChallengeHandlerBase)) {
        }
        protected ChallengeHandlerBase(IHttpWebRequestFactory webRequestFactory, string name) :
            this(webRequestFactory, name, ChallengeHandlerOptions.Default) {
        }
        protected ChallengeHandlerBase(IHttpWebRequestFactory webRequestFactory, IChallengeHandlerOptions options) :
         this(webRequestFactory, nameof(ChallengeHandlerBase), options) {
        }
        protected ChallengeHandlerBase(IHttpWebRequestFactory webRequestFactory, string name, IChallengeHandlerOptions options) {

            if (webRequestFactory is null)
                throw new ArgumentNullException(nameof(webRequestFactory));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            Name = name;
            this.webRequestFactory = webRequestFactory;
            this.options = options;

        }

        protected sealed override IHttpWebResponse Send(IHttpWebRequest request, CancellationToken cancellationToken) {

            try {

                // Set the cookies and user agent on the request to the ones we've cached.

                if (options.RememberCookies && solutionCache.TryGet(request.RequestUri, out IChallengeSolution solution))
                    ApplySolutionToRequest(request, solution);

                return base.Send(request, cancellationToken);

            }
            catch (WebException webEx) {

                // Even though 1020 "Access Denied" errors can't be "solved", they're sometimes the result of Cloudflare detecting something unusual about the request (e.g. header order).
                // It's worth letting the solver make an attempt in case it's able to have the request go through successfully.

                bool isCloudflareDetected = webEx.Response is object && CloudflareUtilities.IsProtectionDetected(webEx.Response);

                if (!isCloudflareDetected)
                    throw;

                // We've encountered a challenge, so we need to pass responsibility off to the derived class.
                // We still want delegating handlers nested further down to be able to see the request before we get a response, so we'll create a new request that returns the solver's response.

                IHttpWebRequest solverRequest = new ChallengeHandlerHttpWebRequest(request, r => {

                    IHttpWebResponse response = GetChallengeResponse(r, webEx, cancellationToken);

                    // The resulting HttpWebResponse should be a ChallengeHttpWebResponse unless the derived class decided to return something else.
                    // It's also possible that other delegating handlers along the line have wrapped the response object (e.g. "HtmlMetaElementHandler").
                    // This is why the response object is cast/and the solution accessed immediately here instead of after the call to "base.Send".

                    // #todo This should really be its own interface accessed through the HandlerHttpWebRequest object to avoid having to downcast.

                    if (response is ChallengeHandlerHttpWebResponse challengeResponse) {

                        // Cache the solution if applicable so that it can be sent with future requests.

                        IChallengeSolution solution = challengeResponse.Solution;

                        // Cache the solution if applicable so that it can be sent with future requests.

                        if (options.RememberCookies)
                            solutionCache.Add(response.ResponseUri, solution);

                        if (!challengeResponse.HasResponseStream) {

                            // Some solvers may not return a response body, so we want to be able to retry the request using the cookies/user agent provided by the solver.

                            response.Close();

                            IHttpWebRequest retryRequest = webRequestFactory.Create(request.RequestUri);

                            ReflectionUtilities.CopyProperties(request, retryRequest, new CopyPropertiesOptions() {
                                IgnoreExceptions = true,
                            });

                            ApplySolutionToRequest(retryRequest, solution);

                            response = base.Send(retryRequest, cancellationToken);

                        }

                    }

                    return response;

                });

                bool solverThrewAnException = false;

                try {

                    return base.Send(solverRequest, cancellationToken);

                }
                catch (Exception challengeHandlerEx) {

                    solverThrewAnException = true;

                    // If the challenge solver throws an exception, we still want the original response so the caller can read it.
                    // Note the the response can be null (e.g. this can happen if FlareSolverr fails to initialize).

                    if (webEx.Response is null)
                        throw new WebException(ExceptionMessages.ChallengeSolverThrewAnException, challengeHandlerEx);
                    else
                        throw new WebException(ExceptionMessages.ChallengeSolverThrewAnException, challengeHandlerEx, webEx.Status, webEx.Response);

                }
                finally {

                    // Make sure to close the response before returning, because it will not be accessible to the caller.
                    // We'll only do this if the solver didn't throw an exception, because otherwise the response is provided in the wrapping WebException.

                    if (!solverThrewAnException && webEx.Response is object)
                        webEx.Response.Close();

                }

            }

        }
        protected abstract IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, Exception exception, CancellationToken cancellationToken);

        // Private members

        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly IChallengeHandlerOptions options = ChallengeHandlerOptions.Default;
        private readonly IChallengeSolutionCache solutionCache = new ChallengeSolutionCache();

        private static void ApplySolutionToRequest(IHttpWebRequest request, IChallengeSolution solution) {

            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (solution is null)
                throw new ArgumentNullException(nameof(solution));

            if (!string.IsNullOrWhiteSpace(solution.UserAgent))
                request.UserAgent = solution.UserAgent;

            // Note that setting the CookieContainer causes the cookie header to be ignored if it was set.
            // If there are any cookies specified, we should add them to the container if we create one.

            if (request.CookieContainer is null) {

                request.CookieContainer = new CookieContainer();

                if (request.Headers.TryGet(HttpRequestHeader.Cookie, out string cookieHeaderValue)) {

                    foreach (Cookie cookie in HttpUtilities.ParseCookies(request.RequestUri, cookieHeaderValue))
                        request.CookieContainer.Add(cookie);

                }

            }

            request.CookieContainer.Add(solution.Cookies);

        }

    }

}