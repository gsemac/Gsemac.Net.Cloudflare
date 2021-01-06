using Gsemac.Net.Cloudflare.FlareSolverr;
using Gsemac.Net.Extensions;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class FlareSolverrIuamChallengeSolver :
        IuamChallengeSolverBase {

        // Public members

        public FlareSolverrIuamChallengeSolver() :
            this(IuamChallengeSolverOptions.Default) {
        }
        public FlareSolverrIuamChallengeSolver(IIuamChallengeSolverOptions options) :
            this(new HttpWebRequestFactory(), options) {
        }
        public FlareSolverrIuamChallengeSolver(IHttpWebRequestFactory webRequestFactory, IIuamChallengeSolverOptions options) :
            base("FlareSolverr IUAM Challenge Solver") {

            if (webRequestFactory is null)
                throw new ArgumentNullException(nameof(webRequestFactory));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.webRequestFactory = webRequestFactory;
            this.options = options;

        }

        public override IIuamChallengeResponse GetChallengeResponse(Uri uri) {

            // Assume that the FlareSolverr proxy server is already running (on port 8191).

            FlareSolverrData flareSolverrData = new FlareSolverrData() {
                Cmd = "request.get",
                Url = uri,
                UserAgent = options.UserAgent,
                MaxTimeout = options.Timeout,
            };

            using (WebClient webClient = webRequestFactory.ToWebClientFactory().Create()) {

                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                Uri flareSolverUri = new Uri($"http://localhost:{FlareSolverrUtilities.DefaultPort}/v1");

                OnLog.Info($"Waiting for response from {flareSolverUri.AbsoluteUri}");

                FlareSolverrResponse response;

                try {

                    string responseJson = webClient.UploadString(flareSolverUri, flareSolverrData.ToString());

                    response = JsonConvert.DeserializeObject<FlareSolverrResponse>(responseJson);

                }
                catch (WebException ex) {

                    if (ex.Status == WebExceptionStatus.ConnectFailure)
                        OnLog.Error($"Failed to connect to FlareSolverr. Make sure that FlareSolverr is running on port {FlareSolverrUtilities.DefaultPort}.");

                    throw ex;

                }

                OnLog.Info($"Got response with status: {response.Status}");

                if (response.Solution?.Status == 200) {

                    // We successfully received a solution.
                    // All we want are the clearance cookies and the user agent.

                    return new IuamChallengeResponse(response.Solution.UserAgent, ExtractClearanceCookies(response.Solution.Cookies));

                }
                else
                    return IuamChallengeResponse.Failed;

            }

        }

        // Private members

        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly IIuamChallengeSolverOptions options;

        private CookieCollection ExtractClearanceCookies(CookieCollection cookies) {

            Cookie cfduid = cookies["__cfduid"];
            Cookie cf_clearance = cookies["cf_clearance"];

            if (!(cfduid is null || cf_clearance is null)) {

                CookieCollection cfCookies = new CookieCollection {
                        cfduid,
                        cf_clearance
                    };

                return cfCookies;

            }

            return new CookieCollection();

        }

    }

}