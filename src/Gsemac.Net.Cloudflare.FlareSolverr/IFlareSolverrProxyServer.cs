using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrProxyServer :
        IDisposable {

        event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        event DownloadFileCompletedEventHandler DownloadFileCompleted;

        bool Start();
        bool Stop();

        IFlareSolverrResponse GetResponse(IFlareSolverrCommand command);

    }

}