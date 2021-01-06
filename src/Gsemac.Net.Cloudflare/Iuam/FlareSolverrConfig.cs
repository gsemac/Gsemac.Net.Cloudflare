namespace Gsemac.Net.Cloudflare.Iuam {

    public class FlareSolverrConfig :
        IFlareSolverrConfig {

        public string FlareSolverrDirectoryPath { get; set; }
        public string NodeJsDirectoryPath { get; set; }
        public IHttpWebRequestFactory WebRequestFactory { get; set; } = new HttpWebRequestFactory();
        public bool AutoDownload { get; set; } = true;
        public bool AutoInstall { get; set; } = true;

    }

}