using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrOptions :
        IFlareSolverrOptions {

        public bool AutoUpdateEnabled { get; set; } = true;
        public bool IgnoreUpdateErrors { get; set; } = true;
        public string FlareSolverrDirectoryPath { get; set; }
        public string FlareSolverrFileName { get; set; }
        public string UserAgent { get; set; }
        public bool UseSession { get; set; } = true;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public bool LogHtml { get; set; } = false;
        public bool Headless { get; set; } = true;
        public TimeSpan BrowserTimeout { get; set; } = TimeSpan.Zero;
        public string TestUrl { get; set; }
        public int Port { get; set; } = FlareSolverrUtilities.DefaultPort;

        public static FlareSolverrOptions Default => new FlareSolverrOptions();

    }

}