using System;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class IuamChallengeHandler :
        DelegatingWebRequestHandler {

        // Public members

        public IuamChallengeHandler(IIuamChallengeSolverFactory challengeSolverFactory, IHttpWebRequestFactory httpWebRequestFactory) {

            if (challengeSolverFactory is null)
                throw new ArgumentNullException(nameof(challengeSolverFactory));

            this.challengeSolverFactory = challengeSolverFactory;
            this.httpWebRequestFactory = httpWebRequestFactory;

        }

        // Protected members

        protected override WebResponse Send(WebRequest request, CancellationToken cancellationToken) {

            try {

                return base.Send(request, cancellationToken);

            }
            catch (WebException ex) {

                bool isCloudflareDetected = ex.Response is object && CloudflareUtilities.IsProtectionDetected(ex.Response);
                bool isSolvableChallenge = isCloudflareDetected && CloudflareUtilities.GetProtectionType(ex.Response) != ProtectionType.AccessDenied;

                if (!isSolvableChallenge)
                    throw;

                try {

                    IIuamChallengeSolver challengeSolver = challengeSolverFactory.Create();

                    IuamChallengeSolverHttpWebRequest challengeSolverWebRequest = new IuamChallengeSolverHttpWebRequest(request.RequestUri, ex, challengeSolver);

                    WebResponse response = base.Send(challengeSolverWebRequest, cancellationToken);

                    if (response is IuamChallengeSolverHttpWebResponse httpWebResponse && !httpWebResponse.ChallengeResponse.HasResponseStream && httpWebResponse.Cookies.Count > 0) {

                        // Not all solvers return a body (some of them just return clearance cookies).
                        // If we didn't get a body, retry the request with the new cookies.

                        IHttpWebRequest newHttpWebRequest = httpWebRequestFactory.Create(request.RequestUri);

                        newHttpWebRequest.UserAgent = httpWebResponse.ChallengeResponse.UserAgent;

                        newHttpWebRequest.CookieContainer.Add(httpWebResponse.Cookies);

                        // Close the previous response before we make a new one.

                        response.Close();

                        return base.Send((WebRequest)newHttpWebRequest, cancellationToken);

                    }
                    else {

                        return response;

                    }

                }
                finally {

                    // Make sure to close the response before returning, because it will not be accessible to the caller.

                    if (ex.Response is object)
                        ex.Response.Close();

                }

            }

        }

        // Private members

        private readonly IIuamChallengeSolverFactory challengeSolverFactory;
        private readonly IHttpWebRequestFactory httpWebRequestFactory;

    }

}