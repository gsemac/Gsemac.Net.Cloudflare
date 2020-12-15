using Gsemac.Net.WebBrowsers;
using Gsemac.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gsemac.Net.Cloudflare {

    public class AutoWebBrowserIIuamChallengeSolver :
        IuamChallengeSolverBase {

        // Public members

        public AutoWebBrowserIIuamChallengeSolver(IWebBrowserInfo webBrowserInfo) :
            this(webBrowserInfo, new IuamChallengeSolverOptions()) {
        }
        public AutoWebBrowserIIuamChallengeSolver(IWebBrowserInfo webBrowserInfo, IIuamChallengeSolverOptions options) {

            this.webBrowserInfo = webBrowserInfo;
            this.options = options;

        }

        public override IIuamChallengeResponse GetChallengeResponse(Uri uri) {

            AddToChallengeQueue(uri);

            //// Get request headers from the web browser so we have a valid user agent.
            //// The browser will then redirect to the Cloudflare-protected webpage.

            //WebHeaderCollection requestHeaders = WebBrowserUtilities.GetWebBrowserRequestHeaders(webBrowserInfo, options.Timeout,
            //     "<script>window.location.href=\"" + uri.AbsoluteUri + "\";</script>");

            //string userAgent = requestHeaders[HttpRequestHeader.UserAgent];

            //// Wait for clearance cookies to become available.

            //IWebBrowserCookieReader cookieReader = new WebBrowserCookieReader(webBrowserInfo);
            //DateTimeOffset startedWaiting = DateTimeOffset.Now;

            //while (DateTimeOffset.Now - startedWaiting < options.Timeout) {

            //    CookieCollection cookies = cookieReader.GetCookies(uri);

            //    Cookie cfduid = cookies["__cfduid"];
            //    Cookie cf_clearance = cookies["cf_clearance"];

            //    if (!(cfduid is null || cf_clearance is null)) {

            //        CookieCollection cfCookies = new CookieCollection {
            //            cfduid,
            //            cf_clearance
            //        };

            //        return new IuamChallengeResponse(userAgent, cfCookies);

            //    }

            //    Thread.Sleep(TimeSpan.FromSeconds(5));

            //}

            // If we get here, we weren't able to obtain clearance cookies.

            return IuamChallengeResponse.Failed;

        }

        // Private members

        private class QueueData {

            public Uri Uri { get; set; }
            public ManualResetEvent ResetEvent { get; } = new ManualResetEvent(false);
            public IIuamChallengeResponse Response { get; set; } = IuamChallengeResponse.Failed;

        }

        private readonly IWebBrowserInfo webBrowserInfo;
        private readonly IIuamChallengeSolverOptions options;
        private readonly object challengeQueueMutex = new object();
        private readonly List<QueueData> challengeQueue = new List<QueueData>();
        private Uri httpListenerUri;
        private readonly HttpListener httpListener = new HttpListener();

        private void AddToChallengeQueue(Uri uri) {

            lock (challengeQueueMutex) {

                // URIs pointing to the same host will be allowed in the queue more than once.
                // However, when we get a challenge response, it will be given to everyone in the queue with the same hostname.

                challengeQueue.Add(new QueueData {
                    Uri = uri
                });

                // Start the HTTP server if it's not already listening.

                StartHttpListener();

            }

        }
        private string BuildDefaultResponse() {

            StringBuilder sb = new StringBuilder(Properties.Resources.WebBrowserIUAMChallengeSolverWebpage);

            sb.Replace("{PRODUCT_NAME}", "Test.Product");

            return sb.ToString();

        }
        private string BuildHeartbeatResponse() {

            // Return the next item in the queue.

            lock (challengeQueueMutex) {

                QueueData queueData = challengeQueue.FirstOrDefault();

                if (!(queueData is null)) {

                    challengeQueue.RemoveAt(0);

                    return queueData.Uri.AbsoluteUri;

                }

            }

            return string.Empty;

        }
        private void StartHttpListener() {

            if (!httpListener.IsListening) {

                httpListenerUri = new Uri($"http://localhost:{SocketUtilities.GetUnusedPort()}/");

                httpListener.Prefixes.Add(httpListenerUri.AbsoluteUri);

                httpListener.Start();

                Task.Factory.StartNew(() => HttpListenerLoop());

            }

        }
        private void HttpListenerLoop() {

            bool listenForRequests = true;
            DateTimeOffset lastHeartbeat = DateTimeOffset.MinValue;
            string userAgent = string.Empty;

            while (listenForRequests) {

                // If it's been too long since our last heartbeat, open the user's web browser again (they may have closed the tab).

                if ((DateTimeOffset.Now - lastHeartbeat).TotalSeconds > 2.0) {

                    Process.Start(webBrowserInfo.ExecutablePath, httpListenerUri.AbsoluteUri);

                    lastHeartbeat = DateTimeOffset.Now;

                }

                HttpListenerContext context = httpListener.GetContext();

                if (!(context is null)) {

                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    // Store the user agent from the last request.

                    string userAgentKey = request.Headers.Keys.Cast<string>()
                        .Where(key => key.Equals("user-agent", StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(userAgentKey))
                        userAgent = request.Headers[userAgentKey];

                    if (request.HttpMethod.Equals("get", StringComparison.OrdinalIgnoreCase)) {

                        if (request.RawUrl.Equals("/WebBrowserIUAMChallengeSolver.js", StringComparison.OrdinalIgnoreCase)) {

                            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.WebBrowserIUAMChallengeSolver)))
                                ms.CopyTo(response.OutputStream);

                        }
                        else {

                            // Display the default webpage.

                            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(BuildDefaultResponse())))
                                ms.CopyTo(response.OutputStream);

                        }

                    }
                    else if (request.HttpMethod.Equals("post", StringComparison.OrdinalIgnoreCase)) {

                        if (request.RawUrl.Equals("/heartbeat", StringComparison.OrdinalIgnoreCase)) {

                            // Update the last heartbeat.

                            lastHeartbeat = DateTimeOffset.Now;

                            // If there are any URIs in the queue, pass them to the webpage.

                            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(BuildHeartbeatResponse())))
                                ms.CopyTo(response.OutputStream);

                        }

                    }

                    response.Close();

                }

            }

        }

    }

}