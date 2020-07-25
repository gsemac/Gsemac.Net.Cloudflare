using System.Net;

namespace Gsemac.Net.CloudflareUtilities {

    public interface IChallengeResponse {

        string UserAgent { get; }
        CookieCollection Cookies { get; }

        bool Success { get; }

    }

}