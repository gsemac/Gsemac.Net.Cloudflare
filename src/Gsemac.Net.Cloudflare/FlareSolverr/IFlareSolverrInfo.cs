using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal interface IFlareSolverrInfo {

        Version Version { get; }
        string ExecutablePath { get; }

    }

}