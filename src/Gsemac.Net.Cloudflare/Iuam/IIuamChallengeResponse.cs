using System;
using System.Net;

namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IIuamChallengeResponse {

        string UserAgent { get; }
        CookieCollection Cookies { get; }
        Uri ResponseUri { get; }
        string ResponseBody { get; }

        bool Success { get; }

    }

}