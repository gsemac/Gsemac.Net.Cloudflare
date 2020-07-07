namespace Gsemac.CloudflareUtilities {

    public static class CloudflareUtilities {

        public static ChallengeType GetChallengeType(string htmlDocument) {

            if (htmlDocument.Contains("cf-im-under-attack")) {

                return ChallengeType.ImUnderAttack;

            }
            else {

                return ChallengeType.None;

            }

        }

    }

}