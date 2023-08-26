using Gsemac.IO.Extensions;
using Gsemac.IO.Logging;
using Gsemac.Net.Cloudflare.Properties;
using Gsemac.Net.Extensions;
using Gsemac.Net.Http;
using Gsemac.Net.Http.Headers;
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

        public FlareSolverrChallengeHandler(IHttpWebRequestFactory webRequestFactory, IFlareSolverrService flareSolverrProxyServer) :
            this(webRequestFactory, flareSolverrProxyServer, new NullLogger()) {
        }
        public FlareSolverrChallengeHandler(IHttpWebRequestFactory webRequestFactory, IFlareSolverrService flareSolverrProxyServer, IChallengeHandlerOptions options) :
          this(webRequestFactory, flareSolverrProxyServer, options, new NullLogger()) {
        }
        public FlareSolverrChallengeHandler(IHttpWebRequestFactory webRequestFactory, IFlareSolverrService flareSolverrProxyServer, ILogger logger) :
            this(webRequestFactory, flareSolverrProxyServer, ChallengeHandlerOptions.Default, logger) {
        }
        public FlareSolverrChallengeHandler(IHttpWebRequestFactory webRequestFactory, IFlareSolverrService flareSolverrProxyServer, IChallengeHandlerOptions options, ILogger logger) :
            base(webRequestFactory, nameof(FlareSolverrChallengeHandler), options) {

            if (flareSolverrProxyServer is null)
                throw new ArgumentNullException(nameof(flareSolverrProxyServer));

            this.flareSolverrService = flareSolverrProxyServer;
            this.logger = new NamedLogger(logger, Name);

        }

        // Protected members

        protected override IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, Exception exception, CancellationToken cancellationToken) {

            // Assume that the FlareSolverr proxy server is already running (on port 8191).

            string commandName;

            switch (request.Method.ToLowerInvariant()) {

                case "get":
                    commandName = FlareSolverrCommand.GetRequest;
                    break;

                case "post":
                    commandName = FlareSolverrCommand.PostRequest;
                    break;

                default:
                    throw new NotSupportedException(string.Format(ExceptionMessages.RequestMethodNotSupportedWithMethod, request.Method));

            }

            // Build the FlareSolverr command.
            // Avoid setting the user agent, because Cloudflare is able to detect discrepancies.

            FlareSolverrCommand flareSolverrCommand = new FlareSolverrCommand(commandName) {
                Url = request.RequestUri,
                Download = true,
                MaxTimeout = TimeSpan.FromMilliseconds(request.Timeout),
            };

            foreach (IHttpHeader header in request.Headers.GetHeaders()) {

                if (IsHeaderAllowed(header.Name))
                    flareSolverrCommand.Headers[header.Name] = header.Value;

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

                    throw new ArgumentException(string.Format(ExceptionMessages.RequestMustBeDecoratedWithLazyUploadHttpWebRequestDecoratorWithTypeName, nameof(LazyUploadHttpWebRequestDecorator)), nameof(request));

                }

            }

            IFlareSolverrResponse response;

            try {

                response = flareSolverrService.GetResponse(flareSolverrCommand);

            }
            catch (WebException ex) {

                if (ex.Status == WebExceptionStatus.ConnectFailure)
                    throw new FlareSolverrException(string.Format(ExceptionMessages.FailedToConnectToFlareSolverrWithPort, FlareSolverrUtilities.DefaultPort), ex);

                throw new FlareSolverrException(ExceptionMessages.FailedToExecuteFlareSolverrCommand, ex);

            }

            logger.Info($"Got response with status: {response.Status}");

            // I used to check that the response code was 200 instead of 503, but sometimes the response code will be 503 even after a successful bypass.
            // Therefore, it is more reliable to check the status code returned by FlareSolverr rather than the webpage.

            if (response.Status == FlareSolverrResponseStatus.Ok) {

                // We successfully received a solution.

                // The "download" parameter allows us to receive the response as a base64-encoded string. However, this feature was deprecated in FlareSolverr v2.0.0.
                // The parameter is allowed, but the response will not be base64-encoded.

                bool isResponseBase64Encoded = flareSolverrCommand.Download &&
                    response.Version < new Version(2, 0);

                ChallengeHandlerHttpWebResponse challengeResponse = new ChallengeHandlerHttpWebResponse(response.Solution.Url, () => StreamFromFlareSolverrSolution(response.Solution, isResponseBase64Encoded: isResponseBase64Encoded)) {
                    Cookies = response.Solution.Cookies,
                    Success = true, // "Success" is set manually because we may not meet success conditions (sometimes we get a 503 on a successful response or don't get any cookies).
                };

                challengeResponse.SetHeaders(DictionaryToWebHeaderCollection(response.Solution.Headers));
                challengeResponse.SetStatusCode((HttpStatusCode)response.Solution.Status);
                challengeResponse.UserAgent = response.Solution.UserAgent;

                // If the content is actually HTML (which will be the case for images when the "download" parameter isn't passed, for example) change the content-type header accordingly.

                challengeResponse.Headers.TryGetHeader(HttpResponseHeader.ContentType, out string contentType);

                if (!isResponseBase64Encoded && IsHtmlResponse(response.Solution.Response) && (string.IsNullOrWhiteSpace(contentType) || !contentType.StartsWith(HtmlContentType)))
                    challengeResponse.Headers.TrySetHeader(HttpResponseHeader.ContentType, HtmlContentType);

                return challengeResponse;

            }
            else {

                throw new FlareSolverrException(string.Format(ExceptionMessages.FlareSolverrReturnedAFailureResponseWithStatus, response.Status));

            }

        }

        // Private members

        const string HtmlContentType = "text/html";

        private readonly IFlareSolverrService flareSolverrService;
        private readonly ILogger logger;

        private static Stream StreamFromFlareSolverrSolution(FlareSolverrSolution solution, bool isResponseBase64Encoded) {

            string responseStr = solution.Response;

            if (isResponseBase64Encoded)
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
        private static bool IsHtmlResponse(string response) {

            if (string.IsNullOrWhiteSpace(response))
                return false;

            return response.StartsWith("<html", StringComparison.OrdinalIgnoreCase) ||
                response.StartsWith("<!doctype html>", StringComparison.OrdinalIgnoreCase);

        }
        private static bool IsHeaderAllowed(string headerName) {

            switch (headerName.ToLowerInvariant()) {

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
                    return false;

                // Avoid setting a user agent header, because it can cause the handler to fail if Cloudflare detects it differs from the browser.
                // While GET requests may still go through sometimes, it seems to cause significant problems with POST requests.

                case "user-agent":
                    return false;

                default:
                    return true;

            }

        }

    }

}