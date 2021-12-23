namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrOptions :
        IFlareSolverrOptions {

        public bool AutoUpdateEnabled { get; set; } = true;
        public bool IgnoreUpdateErrors { get; set; } = true;
        public string FlareSolverrDirectoryPath { get; set; }
        public string FlareSolverrFileName { get; set; }
        public string UserAgent { get; set; }
        public bool UseSession { get; set; } = true;

        public static FlareSolverrOptions Default => new FlareSolverrOptions();

    }

}