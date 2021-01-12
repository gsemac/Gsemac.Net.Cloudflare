using Gsemac.IO.Logging;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public abstract class FlareSolverrServiceBase :
        IFlareSolverrService {

        // Public members

        public event LogEventHandler Log;

        public abstract bool Start();
        public abstract bool Stop();

        public void Dispose() {

            Dispose(disposing: true);

            System.GC.SuppressFinalize(this);

        }

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper("FlareSolverr Service", Log);

        protected virtual void Dispose(bool disposing) { }

    }

}