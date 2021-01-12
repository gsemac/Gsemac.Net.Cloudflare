namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public interface IFlareSolverrConfig {

        string FlareSolverrDirectoryPath { get; }
        string NodeJsDirectoryPath { get; }
        bool AutoDownload { get; }
        bool AutoInstall { get; }

    }

}