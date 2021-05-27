using Gsemac.Net.Extensions;
using System;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class IuamChallengeHandler :
        DelegatingWebRequestHandler {

        // Public members

        public IuamChallengeHandler(IIuamChallengeSolverFactory challengeSolverFactory) {

            if (challengeSolverFactory is null)
                throw new ArgumentNullException(nameof(challengeSolverFactory));

            this.challengeSolverFactory = challengeSolverFactory;

        }

        // Protected members

        protected override WebResponse Send(WebRequest request, CancellationToken cancellationToken) {

            try {

                return base.Send(request, cancellationToken);

            }
            catch (WebException ex) {

                bool isCloudflareDetected = ex.Response is object && CloudflareUtilities.IsProtectionDetected(ex.Response);

                // Even though 1020 "Access Denied" errors can't be "solved", they're sometimes the result of Cloudflare detecting something unusual about the request (e.g. header order).
                // It's worth letting the solver make an attempt in case it's able to have the request go through successfully.

                bool isSolvableChallenge = isCloudflareDetected; // isCloudflareDetected && CloudflareUtilities.GetProtectionType(ex.Response) != ProtectionType.AccessDenied;

                if (!isSolvableChallenge)
                    throw;

                if (!(request is IHttpWebRequest httpWebRequest))
                    throw;

                try {

                    IIuamChallengeResponse challengeResponse = GetChallengeResponse(request.RequestUri);

                    if (challengeResponse is null || !challengeResponse.Success)
                        throw;

                    if (challengeResponse.HasResponseStream) {

                        // Copy the web request parameters to the new web request.
                        // While IuamChallengeSolverHttpWebRequest will not make use of them, the inner handler might.

                        IHttpWebRequest challengeSolverWebRequest = new IuamChallengeSolverHttpWebRequest(request.RequestUri, challengeResponse)
                            .WithOptions(HttpWebRequestOptions.FromHttpWebRequest(httpWebRequest));

                        return base.Send((WebRequest)challengeSolverWebRequest, cancellationToken);

                    }
                    else if (challengeResponse.Cookies.Count > 0) {

                        // Not all solvers return a body (some of them just return clearance cookies).
                        // If we didn't get a body, retry the request with the new cookies.

                        httpWebRequest.UserAgent = challengeResponse.UserAgent;
                        httpWebRequest.CookieContainer.Add(challengeResponse.Cookies);

                        return base.Send((WebRequest)httpWebRequest, cancellationToken);

                    }
                    else
                        throw;

                }
                finally {

                    // Make sure to close the response before returning, because it will not be accessible to the caller.

                    if (ex.Response is object)
                        ex.Response.Close();

                }

            }

        }

        // Private members

        private class IuamChallengeSolverHttpWebRequest :
            HttpWebRequestBase {

            // Public members

            public IuamChallengeSolverHttpWebRequest(Uri requestUri, IIuamChallengeResponse challengeResponse) :
                base(requestUri) {

                this.challengeResponse = challengeResponse;

            }

            public override WebResponse GetResponse() {

                return new IuamChallengeSolverHttpWebResponse(challengeResponse);

            }

            // Private members

            private readonly IIuamChallengeResponse challengeResponse;

        }

        private class IuamChallengeSolverHttpWebResponse :
            HttpWebResponseBase {

            // Public members

            public IIuamChallengeResponse ChallengeResponse { get; }

            public IuamChallengeSolverHttpWebResponse(IIuamChallengeResponse challengeResponse) :
                base(challengeResponse.ResponseUri, challengeResponse.GetResponseStream()) {

                if (challengeResponse is null)
                    throw new ArgumentNullException(nameof(challengeResponse));

                this.ChallengeResponse = challengeResponse;

                ReadChallengeResponse(challengeResponse);

            }

            // Private members

            private void ReadChallengeResponse(IIuamChallengeResponse challengeResponse) {

                Cookies.Add(challengeResponse.Cookies);

                challengeResponse.Headers.CopyTo(Headers);

                StatusCode = challengeResponse.StatusCode;

            }

        }

        private readonly IIuamChallengeSolverFactory challengeSolverFactory;

        private IIuamChallengeResponse GetChallengeResponse(Uri requestUri) {

            return challengeSolverFactory.Create()?.GetResponse(requestUri);

        }

    }

}