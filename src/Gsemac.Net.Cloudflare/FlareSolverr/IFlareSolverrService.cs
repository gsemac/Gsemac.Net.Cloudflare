using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrService :
        ILogEventSource,
        IDisposable {

        event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        event DownloadFileCompletedEventHandler DownloadFileCompleted;
        event LogEventHandler FlareSolverrLog;

        bool Start();
        bool Stop();

        IFlareSolverrResponse ExecuteCommand(IFlareSolverrCommand command);

    }

}