using Gsemac.IO.Logging;

namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IIuamChallengeSolverFactory :
        ILogEventSource {

        IIuamChallengeSolver Create();

    }

}