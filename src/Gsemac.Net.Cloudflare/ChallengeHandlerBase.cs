using Gsemac.IO.Logging;
using Gsemac.Net.Extensions;
using Gsemac.Reflection;
using System;
using System.Net;
using System.Threading;

namespace Gsemac.Net.Cloudflare {

    public abstract class ChallengeHandlerBase :
        HttpWebRequestHandler {

        // Public members

        public string Name { get; }

        // Protected members

        protected ChallengeHandlerBase() :
            this("Cloudflare IUAM Challenge Solver") {
        }
        protected ChallengeHandlerBase(string name) {

            Name = name;

        }

        protected sealed override IHttpWebResponse Send(IHttpWebRequest request, CancellationToken cancellationToken) {

            try {

                return base.Send(request, cancellationToken);

            }
            catch (WebException webEx) {

                // Even though 1020 "Access Denied" errors can't be "solved", they're sometimes the result of Cloudflare detecting something unusual about the request (e.g. header order).
                // It's worth letting the solver make an attempt in case it's able to have the request go through successfully.

                bool isCloudflareDetected = true; // webEx.Response is object && CloudflareUtilities.IsProtectionDetected(webEx.Response);

                if (!isCloudflareDetected)
                    throw;

                // We've encountered a challenge, so we need to pass resposibility off to the derived class.
                // We still want delegating handlers nested further down to be able to see the request before we get a response, so we'll create a new request that returns the solver's response.

                IHttpWebRequest solverRequest = new HandlerHttpWebRequest(request, r => GetChallengeResponse(r, cancellationToken));

                bool solverThrewAnException = false;

                try {

                    IHttpWebResponse response = base.Send(solverRequest, cancellationToken);

                    if (response is ChallengeHttpWebResponse challengeResponse && !challengeResponse.HasResponseStream) {

                        // Some solvers may not return a response body, so we want to be able to retry the request using the cookies/user agent provided by the solver.

                        // The resulting HttpWebResponse should be a ChallengeHttpWebResponse unless the derived class decided to return something else.
                        // #todo This should really be its own interface accessed through the HandlerHttpWebRequest object to avoid having to downcast.

                        request.CookieContainer.Add(challengeResponse.Cookies);
                        request.UserAgent = challengeResponse.UserAgent;

                        return base.Send(request, cancellationToken);

                    }
                    else {

                        // Simply return the response, because it already has a response body.

                        return response;

                    }

                }
                catch (Exception challengeHandlerEx) {

                    solverThrewAnException = true;

                    // If the challenge solver throws an exception, we still want the original response so the caller can read it.

                    throw new WebException(Properties.ExceptionMessages.ChallengeSolverThrewAnException, challengeHandlerEx, webEx.Status, webEx.Response);

                }
                finally {

                    // Make sure to close the response before returning, because it will not be accessible to the caller.
                    // We'll only do this if the solver didn't throw an exception, because otherwise the response is provided in the wrapping WebException.

                    if (!solverThrewAnException && webEx.Response is object)
                        webEx.Response.Close();

                }

            }

        }
        protected abstract IHttpWebResponse GetChallengeResponse(IHttpWebRequest request, CancellationToken cancellationToken);

        // Private members

        private class HandlerHttpWebRequest :
            HttpWebRequestBase {

            // Public members

            public override WebHeaderCollection Headers {
                get => base.Headers;
                set => SetHeaders(value);
            }

            public HandlerHttpWebRequest(IHttpWebRequest baseRequest, Func<IHttpWebRequest, IHttpWebResponse> webResponseFactory) :
                base(baseRequest.RequestUri) {

                ReflectionUtilities.CopyProperties(baseRequest, this, new CopyPropertiesOptions() {
                    IgnoreExceptions = true,
                });

                this.baseRequest = baseRequest;
                this.webResponseFactory = webResponseFactory;

            }

            public override WebResponse GetResponse() {

                IHttpWebRequest request = this;

                // By wrapping the new request in LazyUploadHttpWebRequestDecorator, challenge solvers can still detect their ability to read the request stream.

                if (baseRequest is LazyUploadHttpWebRequestDecorator)
                    request = new LazyUploadHttpWebRequestDecorator(this, () => baseRequest.GetRequestStream());

                return (WebResponse)webResponseFactory(request);

            }

            // Private members

            private readonly IHttpWebRequest baseRequest;
            private readonly Func<IHttpWebRequest, IHttpWebResponse> webResponseFactory;

            private void SetHeaders(WebHeaderCollection headers) {

                Headers.Clear();

                headers.CopyTo(Headers);

            }

        }

    }

}