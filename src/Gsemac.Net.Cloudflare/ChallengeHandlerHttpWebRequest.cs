using Gsemac.Net.Extensions;
using Gsemac.Net.Http;
using Gsemac.Reflection;
using System;
using System.Net;

namespace Gsemac.Net.Cloudflare {

    internal class ChallengeHandlerHttpWebRequest :
        HttpWebRequestBase {

        // Public members

        public override WebHeaderCollection Headers {
            get => base.Headers;
            set => SetHeaders(value);
        }

        public ChallengeHandlerHttpWebRequest(IHttpWebRequest baseRequest, Func<IHttpWebRequest, IHttpWebResponse> webResponseFactory) :
            base(baseRequest.RequestUri) {

            if (baseRequest is null)
                throw new ArgumentNullException(nameof(baseRequest));

            if (webResponseFactory is null)
                throw new ArgumentNullException(nameof(webResponseFactory));

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