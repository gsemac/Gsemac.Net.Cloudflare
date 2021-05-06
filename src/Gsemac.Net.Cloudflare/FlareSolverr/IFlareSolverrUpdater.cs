using Gsemac.IO.Logging;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrUpdater :
        ILogEventSource {

        event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        event DownloadFileCompletedEventHandler DownloadFileCompleted;

        IFlareSolverrInfo Update();

    }

}