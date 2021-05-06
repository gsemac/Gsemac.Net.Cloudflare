namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrOptions {

        bool AutoUpdateEnabled { get; }
        bool IgnoreUpdateErrors { get; }
        string FlareSolverrDirectoryPath { get; }

    }

}