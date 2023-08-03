using Gsemac.Core;
using Gsemac.IO.Logging;
using Gsemac.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Gsemac.Net.Cloudflare.Cloudscraper {

    public class CloudscraperChallengeHandler :
        ChallengeHandlerBase {

        // Public members

        public CloudscraperChallengeHandler(IHttpWebRequestFactory webRequestFactory, ICloudscraperOptions cloudscraperOptions) :
            this(webRequestFactory, cloudscraperOptions, new NullLogger()) {
        }
        public CloudscraperChallengeHandler(IHttpWebRequestFactory webRequestFactory, ICloudscraperOptions cloudscraperOptions, IChallengeHandlerOptions challengeHandlerOptions) :
            this(webRequestFactory, cloudscraperOptions, challengeHandlerOptions, new NullLogger()) {
        }
        public CloudscraperChallengeHandler(IHttpWebRequestFactory webRequestFactory, ICloudscraperOptions cloudscraperOptions, ILogger logger) :
             this(webRequestFactory, cloudscraperOptions, ChallengeHandlerOptions.Default, logger) {
        }
        public CloudscraperChallengeHandler(IHttpWebRequestFactory webRequestFactory, ICloudscraperOptions cloudscraperOptions, IChallengeHandlerOptions challengeHandlerOptions, ILogger logger) :
             base(webRequestFactory, nameof(CloudscraperChallengeHandler), challengeHandlerOptions) {

            if (cloudscraperOptions is null)
                throw new ArgumentNullException(nameof(cloudscraperOptions));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            this.cloudscraperOptions = cloudscraperOptions;
            this.logger = new NamedLogger(logger, Name);

        }

        protected override IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, Exception exception, CancellationToken cancellationToken) {

            string url = request.RequestUri.AbsoluteUri;

            if (!System.IO.File.Exists(cloudscraperOptions.CloudscraperExecutablePath)) {

                logger.Error($"cloudscraper was not found at '{cloudscraperOptions.CloudscraperExecutablePath}'");

                throw new System.IO.FileNotFoundException(cloudscraperOptions.CloudscraperExecutablePath);

            }

            CmdArgumentsBuilder argumentsBuilder = new CmdArgumentsBuilder()
                .WithArgument(url);

            if (!string.IsNullOrWhiteSpace(request.UserAgent))
                argumentsBuilder.WithArgument("--user-agent", request.UserAgent);

            if (request.Proxy is object)
                argumentsBuilder.WithArgument("--proxy", request.Proxy.GetProxy(new Uri(url)).AbsoluteUri);

            ProcessStartInfo startInfo = new ProcessStartInfo() {
                FileName = cloudscraperOptions.CloudscraperExecutablePath,
                Arguments = argumentsBuilder.ToString(),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            const int timeout = 1000 * 60; // 1 minute
            StringBuilder output = new StringBuilder();

            lock (cloudscraperMutex) {

                logger.Info($"Getting Cloudflare clearance cookies for {url}");

                using (Process process = new Process() { StartInfo = startInfo })
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false)) {

                    process.OutputDataReceived += (sender, e) => {

                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else {

                            output.Append(e.Data);

                        }

                    };
                    process.ErrorDataReceived += (sender, e) => {

                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            logger.Error(e.Data);

                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout)) {

                        logger.Info($"Process exited with code {process.ExitCode}");

                    }
                    else {

                        logger.Error("Process timed out");

                    }

                }

            }

            logger.Info($"Got response: {output}");

            // The output will be a JSON array.
            // e.g.: [{"__cfduid": "XXX", "cf_clearance": "XXX"}, "XXX"]

            try {

                CookieCollection cookies = new CookieCollection();
                string userAgent = "";

                JToken resultJson = JArray.Parse(output.ToString());

                foreach (JToken token in resultJson) {

                    if (token.Type == JTokenType.Object) {

                        foreach (JProperty property in token.OfType<JProperty>())
                            cookies.Add(new Cookie(property.Name, property.Value.ToString()));

                    }
                    else if (token.Type == JTokenType.String)
                        userAgent = token.Value<string>();

                }

                return new ChallengeHandlerHttpWebResponse(request.RequestUri, string.Empty) {
                    Cookies = cookies,
                    UserAgent = userAgent,
                };

            }
            catch (Exception ex) {

                logger.Error(ex.ToString());

                throw ex;

            }

        }

        // Private members

        private static readonly object cloudscraperMutex = new object();

        private readonly ICloudscraperOptions cloudscraperOptions;
        private readonly ILogger logger;

    }

}