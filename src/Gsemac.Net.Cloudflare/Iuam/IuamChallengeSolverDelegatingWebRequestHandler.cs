using Gsemac.IO.Logging;
using System;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class IuamChallengeSolverDelegatingWebRequestHandler :
        DelegatingWebRequestHandler,
        ILogEventSource {

        // Public members

        public event LogEventHandler Log;

        public IuamChallengeSolverDelegatingWebRequestHandler(IIuamChallengeSolverFactory challengeSolverFactory, IHttpWebRequestFactory httpWebRequestFactory) {

            if (challengeSolverFactory is null)
                throw new ArgumentNullException(nameof(challengeSolverFactory));

            this.challengeSolverFactory = challengeSolverFactory;
            this.httpWebRequestFactory = httpWebRequestFactory;

        }

        // Protected members

        protected LogEventHandlerWrapper OnLog => new LogEventHandlerWrapper(Log, "Delegating Handler");

        protected override WebResponse Send(WebRequest request, CancellationToken cancellationToken) {

            try {

                return base.Send(request, cancellationToken);

            }
            catch (WebException ex) {

                if (ex.Response is null || !CloudflareUtilities.IsProtectionDetected(ex.Response))
                    throw ex;

                try {

                    OnLog.Warning($"Cloudflare detected on {request.RequestUri.AbsoluteUri}");

                    IIuamChallengeSolver challengeSolver = challengeSolverFactory.Create();
                    IuamChallengeSolverHttpWebRequest challengeSolverWebRequest = new IuamChallengeSolverHttpWebRequest(request.RequestUri, challengeSolver);

                    WebResponse response = base.Send(challengeSolverWebRequest, cancellationToken);

                    // Not all solvers return a body (some of them just return clearance cookies).
                    // If we didn't get a body, retry the request with the new cookies.

                    if (response is IuamChallengeSolverHttpWebResponse httpWebResponse && !httpWebResponse.ChallengeResponse.HasResponseStream) {

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
                catch (Exception solverEx) {

                    throw new AggregateException(solverEx, ex);

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