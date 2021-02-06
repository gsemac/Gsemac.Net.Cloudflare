using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal interface IFlareSolverrVersionInfo {

        Version Version { get; }
        string ExecutablePath { get; }

    }

}