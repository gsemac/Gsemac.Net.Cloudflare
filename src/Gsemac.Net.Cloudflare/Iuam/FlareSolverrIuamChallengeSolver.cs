using Newtonsoft.Json;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class FlareSolverrIuamChallengeSolver :
        IuamChallengeSolverBase {

        // Public members

        public const int FlareSolverrPort = 8191;

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

            Console.WriteLine(flareSolverrData.ToString());

            using (WebClient webClient = new WebClientFactory(webRequestFactory).CreateWebClient()) {

                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                Uri flareSolverUri = new Uri($"http://localhost:{FlareSolverrPort}/v1");

                string responseJson = webClient.UploadString(flareSolverUri, flareSolverrData.ToString());
                FlareSolverrResponse response = JsonConvert.DeserializeObject<FlareSolverrResponse>(responseJson);

                if (response.Solution?.Status == 200) {

                    // We successfully received a solution.
                    // All we want are the clearance cookies and the user agent.

                    return new IuamChallengeResponse(response.Solution.UserAgent, response.Solution.Cookies);

                }
                else
                    return IuamChallengeResponse.Failed;

            }

        }

        // Private members

        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly IIuamChallengeSolverOptions options;

    }

}