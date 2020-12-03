using Gsemac.Net.WebBrowsers;
using System;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare {

    public class WebBrowserIIuamChallengeSolver :
        IuamChallengeSolverBase {

        // Public members

        public WebBrowserIIuamChallengeSolver(IWebBrowserInfo webBrowserInfo) :
            this(webBrowserInfo, new IuamChallengeSolverOptions()) {
        }
        public WebBrowserIIuamChallengeSolver(IWebBrowserInfo webBrowserInfo, IIuamChallengeSolverOptions options) {

            this.webBrowserInfo = webBrowserInfo;
            this.options = options;

        }

        public override IIuamChallengeResponse GetChallengeResponse(Uri uri) {

            // Get request headers from the web browser so we have a valid user agent.
            // The browser will then redirect to the Cloudflare-protected webpage.
            
            WebHeaderCollection requestHeaders = WebBrowserUtilities.GetWebBrowserRequestHeaders(webBrowserInfo, options.Timeout,
                 "<script>window.location.href=\"" + uri.AbsoluteUri + "\";</script>");

            string userAgent = requestHeaders[HttpRequestHeader.UserAgent];

            // Wait for clearance cookies to become available.

            IWebBrowserCookieReader cookieReader = new WebBrowserCookieReader(webBrowserInfo);
            DateTimeOffset startedWaiting = DateTimeOffset.Now;

            while (DateTimeOffset.Now - startedWaiting < options.Timeout) {

                CookieCollection cookies = cookieReader.GetCookies(uri);

                Cookie cfduid = cookies["__cfduid"];
                Cookie cf_clearance = cookies["cf_clearance"];

                if (!(cfduid is null || cf_clearance is null)) {

                    CookieCollection cfCookies = new CookieCollection {
                        cfduid,
                        cf_clearance
                    };

                    return new IuamChallengeResponse(userAgent, cfCookies);

                }

                Thread.Sleep(TimeSpan.FromSeconds(5));

            }

            // If we get here, we weren't able to obtain clearance cookies.

            return IuamChallengeResponse.Failed;

        }

        // Private members

        private readonly IWebBrowserInfo webBrowserInfo;
        private readonly IIuamChallengeSolverOptions options;

    }

}