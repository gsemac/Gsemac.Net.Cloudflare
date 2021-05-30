using Gsemac.IO.Logging;

namespace Gsemac.Net.Cloudflare {

    public interface IChallengeSolverFactory :
        ILogEventSource {

        IChallengeSolver Create();

    }

}