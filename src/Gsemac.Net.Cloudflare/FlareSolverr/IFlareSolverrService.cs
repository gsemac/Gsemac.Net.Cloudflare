using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrService :
        ILoggable,
        IDisposable {

        event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        event DownloadFileCompletedEventHandler DownloadFileCompleted;

        bool Start();
        bool Stop();

    }

}