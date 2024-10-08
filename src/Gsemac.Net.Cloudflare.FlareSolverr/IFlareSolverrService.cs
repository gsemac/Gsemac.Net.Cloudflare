﻿using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrService :
        IDisposable {

        event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        event DownloadFileCompletedEventHandler DownloadFileCompleted;

        bool Start();
        bool Stop();

        IFlareSolverrResponse GetResponse(IFlareSolverrRequest request);

    }

}