using System.Collections.Generic;

namespace Gsemac.Net.CloudflareUtilities {

    public interface IChallengeResponse {

        string UserAgent { get; }
        IDictionary<string, string> Cookies { get; }

        bool Success { get; }

    }

}