namespace Gsemac.CloudflareUtilities {

    public interface IChallengeSolver {

        IChallengeResponse GetChallengeResponse(string url);

    }

}