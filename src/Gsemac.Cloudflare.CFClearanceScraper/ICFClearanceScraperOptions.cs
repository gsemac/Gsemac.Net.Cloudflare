using System;

namespace Gsemac.Cloudflare.CFClearanceScraper {

    public interface ICFClearanceScraperOptions {

        /// <summary>
        /// The maximum number of browsers that can be opened at the same time.
        /// </summary>
        int BrowserLimit { get; }
        /// <summary>
        /// The maximum time a transaction will take.
        /// </summary>
        TimeSpan Timeout { get; }
        string AuthToken { get; }
        int Port { get; }

    }

}