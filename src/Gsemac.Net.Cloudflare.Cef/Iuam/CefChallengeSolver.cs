using CefSharp;
using CefSharp.OffScreen;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare.Iuam {

    public class CefChallengeSolver :
        ChallengeSolverBase {

        // Public members

        public CefChallengeSolver(CefChallengeSolverOptions options) :
            base("CEF IUAM Challenge Solver") {

            this.options = options;

        }

        public override IChallengeResponse GetResponse(Uri uri) {

            IChallengeResponse result = null;

            try {

                string url = uri.AbsoluteUri;

                lock (lockObject) {

                    InitializeCef(options);

                    OnLog.Info("Instantiating web browser");

                    using (ChromiumWebBrowser browser = new ChromiumWebBrowser()) {

                        browser.FrameLoadEnd += FrameLoadEnd;
                        browser.BrowserInitialized += BrowserInitialized;
                        browser.AddressChanged += AddressChanged;

                        OnLog.Info("Waiting for browser initialization");

                        waitHandle.WaitOne(options.Timeout);

                        OnLog.Info($"Loading webpage");

                        browser.Load(url);

                        if (waitHandle.WaitOne(options.Timeout)) {

                            // The page was loaded successfully, so extract the cookies.

                            OnLog.Info($"Solved challenge successfully");

                            result = new ChallengeResponse(uri, string.Empty) {
                                UserAgent = GetUserAgent(browser),
                                Cookies = GetCookies(url, browser),
                            };

                        }
                        else {

                            OnLog.Error($"Failed to solve challenge (timeout)");

                        }

                    }

                }

            }
            catch (Exception ex) {

                OnLog.Error(ex.ToString());

                throw ex;

            }
            finally {

                ShutdownCef();

            }

            return result;

        }

        // Private members

        private class CookieVisitor :
            ICookieVisitor {

            // Public members

            public CookieCollection Cookies { get; } = new CookieCollection();

            public void Dispose() {
            }
            public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie) {

                System.Net.Cookie netCookie = new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain) {
                    Secure = cookie.Secure,
                    HttpOnly = cookie.HttpOnly
                };

                if (cookie.Expires.HasValue)
                    netCookie.Expires = cookie.Expires.Value;

                Cookies.Add(netCookie);

                return true;

            }

        }

        private readonly CefChallengeSolverOptions options;
        private readonly object lockObject = new object();
        private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        private void InitializeCef(CefChallengeSolverOptions options) {

            if (!CefSharp.Cef.IsInitialized) {

                OnLog.Info("Initializing CEF");

                string browserSubprocessPath = string.IsNullOrEmpty(options.BrowserSubprocessPath) ?
                    Path.GetFullPath("CefSharp.BrowserSubprocess.exe") :
                    options.BrowserSubprocessPath;

                var settings = new CefSettings {
                    BrowserSubprocessPath = browserSubprocessPath,
                    LogSeverity = LogSeverity.Disable
                };

                CefSharp.Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);

            }

        }
        private void ShutdownCef() {

            OnLog.Info("Shutting down CEF");

            CefSharp.Cef.Shutdown();

        }

        private string GetUserAgent(ChromiumWebBrowser browser) {

            JavascriptResponse result = browser.EvaluateScriptAsync("navigator.userAgent").Result;

            return result.Success ? result.Result.ToString() : string.Empty;

        }
        private CookieCollection GetCookies(string url, ChromiumWebBrowser browser) {

            using (CookieVisitor visitor = new CookieVisitor()) {

                browser.GetCookieManager().VisitUrlCookies(url, true, visitor);

                return visitor.Cookies;

            }

        }

        private void BrowserInitialized(object sender, EventArgs e) {

            waitHandle.Set();

        }
        private void FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {

            if (e.Frame.IsMain && sender is ChromiumWebBrowser browser) {

                browser.GetSourceAsync().ContinueWith(task => {

                    if (CloudflareUtilities.GetProtectionType(task.Result) == ProtectionType.None)
                        waitHandle.Set();

                });

            }

        }
        private void AddressChanged(object sender, AddressChangedEventArgs e) {

            OnLog.Info($"Navigating to {e.Address}");

        }

    }

}