﻿using System.Threading;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrUpdater {

        event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        event DownloadFileCompletedEventHandler DownloadFileCompleted;

        IFlareSolverrVersionInfo UpgradeToLatestVersion(CancellationToken cancellationToken);

    }

}