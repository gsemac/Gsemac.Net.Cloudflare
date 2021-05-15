using Gsemac.IO.Logging;
using System.Threading;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrUpdater :
        ILogEventSource {

        event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        event DownloadFileCompletedEventHandler DownloadFileCompleted;

        IFlareSolverrInfo Update(CancellationToken cancellationToken);

    }

}