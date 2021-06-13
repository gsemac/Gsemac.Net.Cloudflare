using Gsemac.Core;
using Gsemac.Net.Extensions;
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

        public CloudscraperChallengeHandler(ICloudscraperOptions cloudscraperOptions) :
            base("Cloudscraper IUAM Challenge Solver") {

            this.cloudscraperOptions = cloudscraperOptions;

        }

        protected override IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, CancellationToken cancellationToken) {

            string url = request.RequestUri.AbsoluteUri;

            if (!System.IO.File.Exists(cloudscraperOptions.CloudscraperExecutablePath)) {

                OnLog.Error($"cloudscraper was not found at '{cloudscraperOptions.CloudscraperExecutablePath}'");

                throw new System.IO.FileNotFoundException(cloudscraperOptions.CloudscraperExecutablePath);

            }

            CmdArgumentsBuilder argumentsBuilder = new CmdArgumentsBuilder()
                .WithArgument(url);

            if (!string.IsNullOrWhiteSpace(request.UserAgent))
                argumentsBuilder.AddArgument("--user-agent", request.UserAgent);

            if (request.Proxy is object)
                argumentsBuilder.AddArgument("--proxy", request.Proxy.GetProxy(new Uri(url)).AbsoluteUri);

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

                OnLog.Info($"Getting Cloudflare clearance cookies for {url}");

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
                            OnLog.Error(e.Data);

                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout)) {

                        OnLog.Info($"Process exited with code {process.ExitCode}");

                    }
                    else {

                        OnLog.Error("Process timed out");

                    }

                }

            }

            OnLog.Info($"Got response: {output}");

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

                return new ChallengeHttpWebResponse(request.RequestUri, string.Empty) {
                    Cookies = cookies,
                    UserAgent = userAgent,
                };

            }
            catch (Exception ex) {

                OnLog.Error(ex.ToString());

                throw ex;

            }

        }

        // Private members

        private static readonly object cloudscraperMutex = new object();

        private readonly ICloudscraperOptions cloudscraperOptions;

    }

}