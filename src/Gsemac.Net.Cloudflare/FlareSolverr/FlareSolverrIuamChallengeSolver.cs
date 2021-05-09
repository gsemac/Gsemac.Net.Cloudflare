using Gsemac.Net.Cloudflare.Iuam;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrIuamChallengeSolver :
        IuamChallengeSolverBase {

        // Public members

        public FlareSolverrIuamChallengeSolver(IFlareSolverrService flareSolverrService) :
            this(flareSolverrService, IuamChallengeSolverOptions.Default) {
        }
        public FlareSolverrIuamChallengeSolver(IFlareSolverrService flareSolverrService, IIuamChallengeSolverOptions options) :
            base("FlareSolverr IUAM Challenge Solver") {

            if (flareSolverrService is null)
                throw new ArgumentNullException(nameof(flareSolverrService));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.flareSolverrService = flareSolverrService;
            this.options = options;

        }

        public override IIuamChallengeResponse GetResponse(Uri uri) {

            // Assume that the FlareSolverr proxy server is already running (on port 8191).

            FlareSolverrCommand getCommand = new FlareSolverrCommand("request.get") {
                Url = uri,
                UserAgent = options.UserAgent,
                MaxTimeout = options.Timeout,
            };

            IFlareSolverrResponse response;

            try {

                response = flareSolverrService.ExecuteCommand(getCommand);

            }
            catch (WebException ex) {

                if (ex.Status == WebExceptionStatus.ConnectFailure)
                    OnLog.Error($"Failed to connect to FlareSolverr. Make sure that FlareSolverr is running on port {FlareSolverrUtilities.DefaultPort}.");

                throw ex;

            }

            OnLog.Info($"Got response with status: {response.Status}");

            // I used to check that the response code was 200 instead of 503, but sometimes the response code will be 503 even after a successful bypass.
            // Therefore, it is more reliable to check the status code returned by FlareSolverr rather than the webpage.

            if (response.Status?.Equals("ok", StringComparison.OrdinalIgnoreCase) ?? false) {

                // We successfully received a solution.
                // All we want are the clearance cookies and the user agent.

                return new IuamChallengeResponse(uri) {
                    UserAgent = response.Solution.UserAgent,
                    Cookies = response.Solution.Cookies,
                    ResponseUri = response.Solution.Url,
                    ResponseBody = response.Solution.Response,
                };

            }
            else
                return IuamChallengeResponse.Failed;

        }

        // Private members

        private readonly IFlareSolverrService flareSolverrService;
        private readonly IIuamChallengeSolverOptions options;

    }

}