using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrInfo {

        Version Version { get; }
        string ExecutablePath { get; }

    }

}