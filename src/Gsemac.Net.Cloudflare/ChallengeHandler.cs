using Gsemac.Net.Extensions;
using System;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare {

    public class ChallengeHandler :
        DelegatingWebRequestHandler {

        // Public members

        public ChallengeHandler(IChallengeSolverFactory challengeSolverFactory) {

            if (challengeSolverFactory is null)
                throw new ArgumentNullException(nameof(challengeSolverFactory));

            this.challengeSolverFactory = challengeSolverFactory;

        }

        // Protected members

        protected override WebResponse Send(WebRequest request, CancellationToken cancellationToken) {

            try {

                return base.Send(request, cancellationToken);

            }
            catch (WebException webEx) {

                // Cloudflare challenges are only relevant for HTTP requests.

                if (!(request is IHttpWebRequest httpWebRequest))
                    throw;

                // Even though 1020 "Access Denied" errors can't be "solved", they're sometimes the result of Cloudflare detecting something unusual about the request (e.g. header order).
                // It's worth letting the solver make an attempt in case it's able to have the request go through successfully.

                bool isCloudflareDetected = webEx.Response is object && CloudflareUtilities.IsProtectionDetected(webEx.Response);

                if (!isCloudflareDetected)
                    throw;

                // Get a response from the challenge solver.

                IChallengeResponse challengeResponse;

                try {

                    challengeResponse = GetChallengeResponse(request.RequestUri);

                }
                catch (Exception challengeSolverEx) {

                    // If the challenge solver throws an exception, we still want the original response so the caller can read it.

                    throw new WebException(Properties.ExceptionMessages.ChallengeSolverThrewAnException, challengeSolverEx, webEx.Status, webEx.Response);

                }

                // If we got a challenge response but it wasn't successful, rethrow an exception.

                if (challengeResponse is null || !challengeResponse.Success || !(challengeResponse.HasResponseStream || challengeResponse.Cookies.Count > 0))
                    throw new WebException(Properties.ExceptionMessages.ChallengeSolverFailed, webEx, webEx.Status, webEx.Response);

                try {

                    if (challengeResponse.HasResponseStream) {

                        // Copy the web request parameters to the new web request.
                        // While IuamChallengeSolverHttpWebRequest will not make use of them, the inner handler might.

                        IHttpWebRequest challengeSolverWebRequest = new IuamChallengeSolverHttpWebRequest(request.RequestUri, challengeResponse)
                            .WithOptions(HttpWebRequestOptions.FromHttpWebRequest(httpWebRequest));

                        return base.Send((WebRequest)challengeSolverWebRequest, cancellationToken);

                    }
                    else {

                        // Not all solvers return a body (some of them just return clearance cookies).
                        // If we didn't get a body, retry the request with the new cookies.
                        // Only do this if we HAVE new cookies in the first place, which is checked for above.

                        httpWebRequest.UserAgent = challengeResponse.UserAgent;
                        httpWebRequest.CookieContainer.Add(challengeResponse.Cookies);

                        return base.Send((WebRequest)httpWebRequest, cancellationToken);

                    }

                }
                finally {

                    // Make sure to close the response before returning, because it will not be accessible to the caller.

                    if (webEx.Response is object)
                        webEx.Response.Close();

                }

            }

        }

        // Private members

        private class IuamChallengeSolverHttpWebRequest :
            HttpWebRequestBase {

            // Public members

            public IuamChallengeSolverHttpWebRequest(Uri requestUri, IChallengeResponse challengeResponse) :
                base(requestUri) {

                this.challengeResponse = challengeResponse;

            }

            public override WebResponse GetResponse() {

                return new IuamChallengeSolverHttpWebResponse(challengeResponse);

            }

            // Private members

            private readonly IChallengeResponse challengeResponse;

        }

        private class IuamChallengeSolverHttpWebResponse :
            HttpWebResponseBase {

            // Public members

            public IChallengeResponse ChallengeResponse { get; }

            public IuamChallengeSolverHttpWebResponse(IChallengeResponse challengeResponse) :
                base(challengeResponse.ResponseUri, challengeResponse.GetResponseStream()) {

                if (challengeResponse is null)
                    throw new ArgumentNullException(nameof(challengeResponse));

                ChallengeResponse = challengeResponse;

                ReadChallengeResponse(challengeResponse);

            }

            // Private members

            private void ReadChallengeResponse(IChallengeResponse challengeResponse) {

                Cookies.Add(challengeResponse.Cookies);

                challengeResponse.Headers.CopyTo(Headers);

                StatusCode = challengeResponse.StatusCode;

            }

        }

        private readonly IChallengeSolverFactory challengeSolverFactory;

        private IChallengeResponse GetChallengeResponse(Uri requestUri) {

            return challengeSolverFactory.Create()?.GetResponse(requestUri);

        }

    }

}