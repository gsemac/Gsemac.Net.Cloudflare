namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrOptions :
        IFlareSolverrOptions {

        public static FlareSolverrOptions Default => new FlareSolverrOptions();

        public string FlareSolverrExecutablePath { get; set; } = "flaresolverr.exe";

    }

}