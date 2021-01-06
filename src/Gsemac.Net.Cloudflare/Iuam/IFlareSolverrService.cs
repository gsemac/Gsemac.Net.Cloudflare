using Gsemac.IO.Logging;
using System;

namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IFlareSolverrService :
        ILoggable,
        IDisposable {

        bool Start(IFlareSolverrConfig config);
        bool Stop();

    }

}