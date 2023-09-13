using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrVersionInfo {

        Version Version { get; }
        string ExecutablePath { get; }

    }

}