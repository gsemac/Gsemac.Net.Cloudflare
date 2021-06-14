﻿using Gsemac.Net.Extensions;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Gsemac.Net.Cloudflare {

    internal class ChallengeHttpWebResponse :
        HttpWebResponseBase {

        // Public members

        public string UserAgent {
            get => Headers[HttpRequestHeader.UserAgent];
            set => Headers[HttpRequestHeader.UserAgent] = value;
        }
        public override CookieCollection Cookies { get; set; } = new CookieCollection();
        public override Uri ResponseUri { get; }
        public override WebHeaderCollection Headers => headers;

        public bool Success {
            get => success ?? GetDefaultSuccess();
            set => success = value;
        }
        public bool HasResponseStream => streamFactory is object;

        public ChallengeHttpWebResponse(Uri responseUri, string responseBody) :
            this(responseUri) {

            if (responseUri is null)
                throw new ArgumentNullException(nameof(responseUri));

            ResponseUri = responseUri;

            if (!string.IsNullOrWhiteSpace(responseBody)) {

                byte[] responseBytes = Encoding.UTF8.GetBytes(responseBody);

                // I deliberately use the string "content-length" instead of the enum value here.
                // Using the enum will automatically make the WebHeaderCollection restricted to response headers.
                // I prefer to leave it open so that challenge solvers can set the user-agent header for the user to access.

                if (!Headers.TryGetHeader("content-length", out _))
                    Headers["content-length"] = responseBytes.Length.ToString(CultureInfo.InvariantCulture);

                streamFactory = () => new MemoryStream(responseBytes);

            }

        }
        public ChallengeHttpWebResponse(Uri responseUri, Func<Stream> streamFactory) :
            this(responseUri) {

            if (responseUri is null)
                throw new ArgumentNullException(nameof(responseUri));

            if (streamFactory is null)
                throw new ArgumentNullException(nameof(streamFactory));

            ResponseUri = responseUri;
            this.streamFactory = streamFactory;

        }

        public void SetHeaders(WebHeaderCollection headers) {

            this.headers = headers;

        }
        public void SetStatusCode(HttpStatusCode statusCode) {

            StatusCode = statusCode;

        }

        public override Stream GetResponseStream() {

            if (streamFactory is null)
                return null;

            return streamFactory();

        }

        // Private members

        private readonly Func<Stream> streamFactory;
        private WebHeaderCollection headers = new WebHeaderCollection();
        private bool? success;

        private ChallengeHttpWebResponse(Uri responseUri) :
            base(responseUri) {

            StatusCode = HttpStatusCode.OK;

        }

        private bool GetDefaultSuccess() {

            // It is still possible to get other status codes on success (e.g. 503), or end up with no response cookies.
            // In such cases, the property should be set manually.

            return StatusCode == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(UserAgent) && Cookies.Count > 0;

        }

    }

}