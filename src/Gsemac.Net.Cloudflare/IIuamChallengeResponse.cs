using System.Net;

namespace Gsemac.Net.Cloudflare {

    public interface IIuamChallengeResponse {

        string UserAgent { get; }
        CookieCollection Cookies { get; }

        bool Success { get; }

    }

}