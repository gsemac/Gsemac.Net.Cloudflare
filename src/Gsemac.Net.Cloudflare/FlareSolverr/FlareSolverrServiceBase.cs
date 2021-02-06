﻿using Gsemac.IO.Logging;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public abstract class FlareSolverrServiceBase :
        IFlareSolverrService {

        // Public members

        public event LogEventHandler Log;
        public event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        public event DownloadFileCompletedEventHandler DownloadFileCompleted;

        public abstract bool Start();
        public abstract bool Stop();

        public void Dispose() {

            Dispose(disposing: true);

            System.GC.SuppressFinalize(this);

        }

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper("FlareSolverr Service", Log);

        protected void OnDownloadFileProgressChanged(object sender, DownloadFileProgressChangedEventArgs e) {

            DownloadFileProgressChanged?.Invoke(this, e);

        }
        protected void OnDownloadFileCompleted(object sender, DownloadFileCompletedEventArgs e) {

            DownloadFileCompleted?.Invoke(this, e);

        }

        protected virtual void Dispose(bool disposing) { }

    }

}