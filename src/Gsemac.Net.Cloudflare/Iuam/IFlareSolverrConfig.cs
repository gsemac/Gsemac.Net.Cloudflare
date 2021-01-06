namespace Gsemac.Net.Cloudflare.Iuam {

    public interface IFlareSolverrConfig {

        string FlareSolverrDirectoryPath { get; }
        string NodeJsDirectoryPath { get; }
        IHttpWebRequestFactory WebRequestFactory { get; }
        bool AutoDownload { get; }
        bool AutoInstall { get; }

    }

}