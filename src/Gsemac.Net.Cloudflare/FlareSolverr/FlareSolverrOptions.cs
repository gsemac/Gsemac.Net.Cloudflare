namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrOptions :
        IFlareSolverrOptions {

        public static FlareSolverrOptions Default => new FlareSolverrOptions();

        public bool AutoUpdateEnabled { get; set; } = true;
        public string FlareSolverrDirectory { get; set; }

    }

}