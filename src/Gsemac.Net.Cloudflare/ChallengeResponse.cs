using Gsemac.Net.Extensions;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Gsemac.Net.Cloudflare {

    public sealed class ChallengeResponse :
        IChallengeResponse {

        // Public members

        public string UserAgent { get; set; }
        public CookieCollection Cookies { get; set; } = new CookieCollection();
        public Uri ResponseUri { get; set; }
        public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        public bool Success {
            get => success ?? GetDefaultSuccess();
            set => success = value;
        }
        public bool HasResponseStream => streamFactory is object;

        public static ChallengeResponse Failed => new ChallengeResponse();

        public ChallengeResponse(Uri requestUri, string responseBody) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseBody);

            if (!Headers.TryGetHeader(HttpResponseHeader.ContentLength, out _))
                Headers[HttpResponseHeader.ContentLength] = responseBytes.Length.ToString(CultureInfo.InvariantCulture);

            ResponseUri = requestUri;
            streamFactory = () => new MemoryStream(responseBytes);

        }
        public ChallengeResponse(Uri requestUri, Func<Stream> streamFactory) {

            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            if (streamFactory is null)
                throw new ArgumentNullException(nameof(streamFactory));

            ResponseUri = requestUri;
            this.streamFactory = streamFactory;

        }

        public Stream GetResponseStream() {

            if (streamFactory is null)
                return null;

            return streamFactory();

        }

        // Private members

        private readonly Func<Stream> streamFactory;
        private bool? success;

        private ChallengeResponse() { }

        private bool GetDefaultSuccess() {

            // It is still possible to get other status codes on success (e.g. 503), or end up with no response cookies.
            // In such cases, the property should be set manually.

            return StatusCode == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(UserAgent) && Cookies.Count > 0;

        }

    }

}