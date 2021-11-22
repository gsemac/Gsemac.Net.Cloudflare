namespace Gsemac.Net.Cloudflare {

    public class ChallengeHandlerOptions :
        IChallengeHandlerOptions {

        public static ChallengeHandlerOptions Default => new ChallengeHandlerOptions();

        public bool RememberCookies { get; set; } = true;

    }

}