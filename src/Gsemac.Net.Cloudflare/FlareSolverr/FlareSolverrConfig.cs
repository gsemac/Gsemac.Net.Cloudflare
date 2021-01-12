namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrConfig :
        IFlareSolverrConfig {

        public string FlareSolverrDirectoryPath { get; set; }
        public string NodeJsDirectoryPath { get; set; }
        public bool AutoDownload { get; set; } = true;
        public bool AutoInstall { get; set; } = true;

    }

}