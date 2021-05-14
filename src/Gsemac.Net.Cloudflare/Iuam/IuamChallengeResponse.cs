using Gsemac.Net.Extensions;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Gsemac.Net.Cloudflare.Iuam {

    public sealed class IuamChallengeResponse :
        IIuamChallengeResponse {

        // Public members

        public string UserAgent { get; set; }
        public CookieCollection Cookies { get; set; } = new CookieCollection();
        public Uri ResponseUri { get; set; }
        public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public bool Success => StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(UserAgent) && Cookies.Count > 0;

        public static IuamChallengeResponse Failed => new IuamChallengeResponse();

        public IuamChallengeResponse(Uri requestUri, string responseBody) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseBody);

            if (!Headers.TryGetHeaderValue(HttpResponseHeader.ContentLength, out _))
                Headers[HttpResponseHeader.ContentLength] = responseBytes.Length.ToString(CultureInfo.InvariantCulture);

            this.ResponseUri = requestUri;
            this.streamFactory = () => new MemoryStream(responseBytes);

        }
        public IuamChallengeResponse(Uri requestUri, Func<Stream> streamFactory) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            if (streamFactory is null)
                throw new ArgumentNullException(nameof(streamFactory));

            this.ResponseUri = requestUri;
            this.streamFactory = streamFactory;

        }

        public Stream GetResponseStream() {

            if (streamFactory is null)
                return null;

            return streamFactory();

        }

        // Private members

        private readonly Func<Stream> streamFactory;

        private IuamChallengeResponse() { }

    }

}