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

        public IuamChallengeSolverDelegatingWebRequestHandler(IIuamChallengeSolverFactory challengeSolverFactory) {

            if (challengeSolverFactory is null)
                throw new ArgumentNullException(nameof(challengeSolverFactory));

            this.challengeSolverFactory = challengeSolverFactory;

        }

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper("Delegating Handler", Log);

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

                    return base.Send(new IuamChallengeSolverHttpWebRequest(request.RequestUri, challengeSolver), cancellationToken);

                }
                catch (Exception solverEx) {

                    throw new AggregateException(solverEx, ex);

                }

            }

        }

        // Private members

        private readonly IIuamChallengeSolverFactory challengeSolverFactory;

    }

}