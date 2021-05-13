using System;
using System.IO;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IIuamChallengeResponse {

        CookieCollection Cookies { get; }
        WebHeaderCollection Headers { get; }
        Uri ResponseUri { get; }
        HttpStatusCode StatusCode { get; }
        string UserAgent { get; }

        bool Success { get; }

        Stream GetResponseStream();

    }

}