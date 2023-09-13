using Gsemac.Net.Cloudflare.FlareSolverr.Json;
using Newtonsoft.Json;
using System;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal interface IFlareSolverrStatus {

        string Message { get; }
        Version Version { get; }
        string UserAgent { get; }

    }

}