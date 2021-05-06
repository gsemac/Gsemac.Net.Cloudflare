namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrUpdaterOptions :
        IFlareSolverrUpdaterOptions {

        public string FlareSolverrDirectoryPath { get; set; }

        public static FlareSolverrUpdaterOptions Default => new FlareSolverrUpdaterOptions();

    }

}