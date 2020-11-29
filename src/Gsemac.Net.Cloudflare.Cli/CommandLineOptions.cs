using CommandLine;

namespace Gsemac.Net.Cloudflare.Cli {

    class CommandLineOptions {

        // Public members

        [Value(0, MetaName = "url", Required = true, HelpText = "The URL to to retrieve clearance cookies from.")]
        public string Url { get; set; }

        [Option("user-agent", HelpText = "The User-Agent header to use when making requests.")]
        public string UserAgent { get; set; }

        [Option("headless", HelpText = "Runs the web driver in headless mode.")]
        public bool Headless { get; set; } = false;

        [Option("firefox", HelpText = "Use the Firefox web driver.")]
        public bool Firefox { get; set; } = false;

        [Option("chrome", HelpText = "Use the Chrome web driver.")]
        public bool Chrome { get; set; } = false;

    }

}