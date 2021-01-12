using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrService :
        ILoggable,
        IDisposable {

        bool Start();
        bool Stop();

    }

}