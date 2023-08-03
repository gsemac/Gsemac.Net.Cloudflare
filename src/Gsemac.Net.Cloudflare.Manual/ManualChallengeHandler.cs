using Gsemac.IO.Logging;
using Gsemac.Net.Cloudflare.Manual.Properties;
using Gsemac.Net.Http;
using Gsemac.Net.Http.Extensions;
using Gsemac.Net.Sockets;
using Gsemac.Net.WebBrowsers;
using Gsemac.Polyfills.System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Gsemac.Net.Cloudflare.Manual {

    public class ManualChallengeHandler :
        ChallengeHandlerBase {

        // Public members

        public ManualChallengeHandler(IHttpWebRequestFactory webRequestFactory) :
            this(webRequestFactory, Logger.Null) {
        }
        public ManualChallengeHandler(IHttpWebRequestFactory webRequestFactory, ILogger logger) :
            this(webRequestFactory, ChallengeHandlerOptions.Default, logger) {
        }
        public ManualChallengeHandler(IHttpWebRequestFactory webRequestFactory, IChallengeHandlerOptions challengeHandlerOptions) :
            this(webRequestFactory, challengeHandlerOptions, Logger.Null) {
        }
        public ManualChallengeHandler(IHttpWebRequestFactory webRequestFactory, IChallengeHandlerOptions challengeHandlerOptions, ILogger logger) :
             base(webRequestFactory, nameof(ManualChallengeHandler), challengeHandlerOptions) {

            if (challengeHandlerOptions is null)
                throw new ArgumentNullException(nameof(challengeHandlerOptions));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            this.logger = new NamedLogger(logger, Name);

        }

        // Protected members

        protected override IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, Exception exception, CancellationToken cancellationToken) {

            if (request is null)
                throw new ArgumentNullException(nameof(request));

            // Get the user's web browser and profile information.

            IWebBrowserInfo browserInfo = WebBrowserInfoFactory.Default.GetDefaultWebBrowser();

            IWebBrowserProfile profileInfo = browserInfo?.GetProfiles()
                    .Where(profile => profile.IsDefault)
                    .FirstOrDefault();

            if (browserInfo is null)
                throw new ChallengeHandlerException(ExceptionMessages.UnableToLocateWebBrowser, exception);

            logger.Info($"Using browser {browserInfo}");

            if (profileInfo is null) {

                logger.Warning($"Could not determine default browser profile ({browserInfo})");

            }
            else {

                logger.Info($"Using browser profile {profileInfo.Identifier}");

            }

            // We will get the browser's user agent, and then redirect to the page we need to access.
            // After that, we will retrieve the clearance cookies from the web browser.

            HttpListener httpListener = new HttpListener();

            string userAgent = string.Empty;

            // Start listening for an incoming request from the user's web browser.

            try {

                int port = SocketUtilities.GetAvailablePort();
                string url = $"http://localhost:{port}/";

                httpListener.Prefixes.Add(url);

                logger.Info($"Starting local HTTP server on {url}");

                httpListener.Start();

                Task listenerTask = TaskEx.Run(() => {

                    HttpListenerContext context = httpListener.GetContext(timeout);

                    if (context is object) {

                        // Store the browser's user agent as part of the solution.

                        userAgent = context.Request.UserAgent;

                        logger.Info($"Got web browser user agent: {userAgent}");

                        // Redirect the browser to the request URI.

                        context.Response.Redirect(request.RequestUri.AbsoluteUri);

                        context.Response.Close();

                    }

                }, cancellationToken);

                WebBrowserUtilities.OpenUrl(url, new OpenUrlOptions() {
                    WebBrowser = browserInfo,
                    Profile = profileInfo,
                });

                listenerTask.Wait((int)timeout.TotalMilliseconds, cancellationToken);

            }
            finally {

                httpListener.Close();

            }

            // If we were successful, wait for clearance cookie(s) to appear.

            if (!string.IsNullOrWhiteSpace(userAgent)) {

                logger.Info($"Reading clearance cookies from browser");

                DateTimeOffset currentTime = DateTimeOffset.Now;

                while (DateTimeOffset.Now - currentTime < timeout) {

                    CookieCollection clearanceCookies = GetCloudflareCookiesFromWebBrowser(request.RequestUri, browserInfo, profileInfo);

                    if (clearanceCookies.Count > 0) {

                        // Clearance cookies were successfully obtained.

                        return new ChallengeHandlerHttpWebResponse(request.RequestUri, string.Empty) {
                            Cookies = clearanceCookies,
                            UserAgent = userAgent,
                        };

                    }

                    // Wait a little while before checking again for the clearance cookies.

                    Thread.Sleep(TimeSpan.FromSeconds(2));

                }

            }

            // We weren't able to obtain clearance cookies.

            throw new ChallengeHandlerException(ExceptionMessages.UnableToReadClearanceCookiesFromBrowser, exception);

        }

        // Private members

        private readonly ILogger logger;

        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(30);

        private CookieCollection GetCloudflareCookiesFromWebBrowser(Uri requestUri, IWebBrowserInfo browserInfo, IWebBrowserProfile browserProfile) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            if (browserInfo is null || browserProfile is null)
                return new CookieCollection();

            // Get cookies for this URI from the user's web browser.

            if (browserProfile is null)
                return new CookieCollection();

            CookieCollection requestCookies = browserProfile.GetCookies()
                 .GetCookies(requestUri);

            // Filter the cookies so we only have the ones relevant to Cloudflare.

            IEnumerable<Cookie> cloudflareCookies = requestCookies.Cast<Cookie>()
                .Where(cookie => CloudflareUtilities.IsCloudflareCookie(cookie))
                ?? Enumerable.Empty<Cookie>();

            CookieCollection cloudflareCookiesCollection = new CookieCollection();

            foreach (Cookie cookie in cloudflareCookies)
                cloudflareCookiesCollection.Add(cookie);

            return cloudflareCookiesCollection;

        }

    }

}