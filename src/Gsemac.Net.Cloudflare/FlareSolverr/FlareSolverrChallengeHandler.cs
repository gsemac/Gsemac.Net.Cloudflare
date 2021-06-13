using Gsemac.IO.Extensions;
using Gsemac.Net.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrChallengeHandler :
        ChallengeHandlerBase {

        // Public members

        public FlareSolverrChallengeHandler(IFlareSolverrService flareSolverrService) :
            base("FlareSolverr IUAM Challenge Solver") {

            if (flareSolverrService is null)
                throw new ArgumentNullException(nameof(flareSolverrService));

            this.flareSolverrService = flareSolverrService;

        }

        // Protected members

        protected override IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, CancellationToken cancellationToken) {

            // Assume that the FlareSolverr proxy server is already running (on port 8191).

            string commandName;

            switch (request.Method.ToLowerInvariant()) {

                case "get":
                    commandName = "request.get";
                    break;

                case "post":
                    commandName = "request.post";
                    break;

                default:
                    throw new NotSupportedException(string.Format(Properties.ExceptionMessages.RequestMethodNotSupportedWithMethod, request.Method));

            }

            FlareSolverrCommand flareSolverrCommand = new FlareSolverrCommand(commandName) {
                Url = request.RequestUri,
                Download = true,
                //UserAgent = request.UserAgent, // Avoid setting the user agent, because Cloudflare can detect discrepancies
                MaxTimeout = TimeSpan.FromMilliseconds(request.Timeout),
            };

            foreach (IHttpHeader header in request.Headers.GetHeaders()) {

                switch (header.Name.ToLowerInvariant()) {

                    // Avoid attempting to set "unsafe" headers.
                    // https://source.chromium.org/chromium/chromium/src/+/master:services/network/public/cpp/header_util.cc;l=25;drc=9e64a1a40f598b893da3b47cfc88cc2cd1a9289d;bpv=1;bpt=1

                    case "content-length":
                    case "host":
                    case "trailer":
                    case "te":
                    case "upgrade":
                    case "cookie2":
                    case "keep-alive":
                    case "transfer-encoding":
                        break;

                    default:
                        flareSolverrCommand.Headers[header.Name] = header.Value;
                        break;

                }

            }

            bool isPostRequest = request.Method.Equals("post", StringComparison.OrdinalIgnoreCase);

            if (isPostRequest) {

                // Disable the "download" parameter, because it will not return a result for POST requests.

                flareSolverrCommand.Download = false;

                // Read the request stream from the request, which requires that a specific decorator is used.

                if (request is LazyUploadHttpWebRequestDecorator) {

                    byte[] postDataBytes = request.GetRequestStream().ToArray();

                    if (postDataBytes is object && postDataBytes.Length > 0)
                        flareSolverrCommand.PostData = Encoding.UTF8.GetString(postDataBytes);

                }
                else {

                    throw new ArgumentException(string.Format(Properties.ExceptionMessages.RequestMustBeDecoratedWithLazyUploadHttpWebRequestDecoratorWithTypeName, nameof(LazyUploadHttpWebRequestDecorator)), nameof(request));

                }

            }

            IFlareSolverrResponse response;

            try {

                response = flareSolverrService.ExecuteCommand(flareSolverrCommand);

            }
            catch (WebException ex) {

                if (ex.Status == WebExceptionStatus.ConnectFailure)
                    throw new FlareSolverrException(string.Format(Properties.ExceptionMessages.FailedToConnectToFlareSolverrWithPort, FlareSolverrUtilities.DefaultPort), ex);

                throw new FlareSolverrException(Properties.ExceptionMessages.FailedToExecuteFlareSolverrCommand, ex);

            }

            OnLog.Info($"Got response with status: {response.Status}");

            // I used to check that the response code was 200 instead of 503, but sometimes the response code will be 503 even after a successful bypass.
            // Therefore, it is more reliable to check the status code returned by FlareSolverr rather than the webpage.

            if (response.Status?.Equals("ok", StringComparison.OrdinalIgnoreCase) ?? false) {

                // We successfully received a solution.
                // "Success" is set manually because we may not meet success conditions (sometimes we get a 503 on a successful response or don't get any cookies).

                ChallengeHttpWebResponse challengeResponse = new ChallengeHttpWebResponse(response.Solution.Url, () => StreamFromFlareSolverrSolution(response.Solution, isBase64: flareSolverrCommand.Download)) {
                    Cookies = response.Solution.Cookies,
                    Success = true,
                };

                challengeResponse.SetHeaders(DictionaryToWebHeaderCollection(response.Solution.Headers));
                challengeResponse.SetStatusCode((HttpStatusCode)response.Solution.Status);
                challengeResponse.UserAgent = response.Solution.UserAgent;

                return challengeResponse;

            }
            else {

                throw new FlareSolverrException(string.Format(Properties.ExceptionMessages.FlareSolverrReturnedAFailureResponseWithStatus, response.Status));

            }

        }

        // Private members

        private readonly IFlareSolverrService flareSolverrService;

        private static Stream StreamFromFlareSolverrSolution(FlareSolverrSolution solution, bool isBase64) {

            string responseStr = solution.Response;

            if (isBase64)
                return StreamFromBase64(responseStr);
            else
                return new MemoryStream(Encoding.UTF8.GetBytes(responseStr));

        }
        private static Stream StreamFromBase64(string base64String) {

            return new MemoryStream(Convert.FromBase64String(base64String));

        }
        private static WebHeaderCollection DictionaryToWebHeaderCollection(IDictionary<string, string> headers) {

            WebHeaderCollection webHeaderCollection = new WebHeaderCollection();

            // FlareSolverr combines multiple headers with the same name (e.g. "set-cookie") into one newline-delimited header.
            // We'll get a "Specified value has invalid CRLF characters" error if we try to use these values directly, so we need to split them.

            foreach (var header in headers) {

                foreach (string headerValue in header.Value.Split('\n'))
                    webHeaderCollection.Add(header.Key, headerValue);

            }

            return webHeaderCollection;

        }

    }

}