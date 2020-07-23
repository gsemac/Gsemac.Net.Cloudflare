namespace Gsemac.Net.CloudflareUtilities {

    public static class CloudflareUtilities {

        public static ChallengeType GetChallengeType(string htmlDocument) {

            if (htmlDocument.Contains("cf-im-under-attack")) {

                return ChallengeType.ImUnderAttack;

            }
            else if (htmlDocument.Contains("cf-captcha-container")) {

                return ChallengeType.CaptchaBypass;

            }
            else {

                return ChallengeType.None;

            }

        }

    }

}