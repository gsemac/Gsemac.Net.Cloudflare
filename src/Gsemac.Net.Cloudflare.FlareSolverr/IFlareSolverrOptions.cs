using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrOptions {

        bool AutoUpdateEnabled { get; }
        bool IgnoreUpdateErrors { get; }
        string FlareSolverrDirectoryPath { get; }
        string FlareSolverrFileName { get; }
        string UserAgent { get; }
        bool UseSession { get; }
        LogLevel LogLevel { get; }
        bool LogHtml { get; }
        bool Headless { get; }
        TimeSpan BrowserTimeout { get; }
        string TestUrl { get; }
        int Port { get; }
        bool SkipPlatformCheck { get; }

    }

}