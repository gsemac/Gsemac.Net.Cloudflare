namespace Gsemac.Net.CloudflareUtilities {

    public static class CloudflareUtilities {

        public static ChallengeType GetChallengeType(string htmlDocument) {

            if (htmlDocument.Contains("cf-im-under-attack\">")) {

                return ChallengeType.ImUnderAttack;

            }
            else if (htmlDocument.Contains("cf-wrapper\">")) {

                if (htmlDocument.Contains("cf-captcha-container\">")) {

                    // This is the same challenge as the "One More Step" captcha.

                    return ChallengeType.CaptchaBypass;

                }
                else if (htmlDocument.Contains(") has banned your IP address (")) {

                    return ChallengeType.AccessDenied;

                }

            }

            return ChallengeType.None;

        }
        public static bool IsChallengeDetected(string htmlDocument) {

            return GetChallengeType(htmlDocument) != ChallengeType.None;

        }

    }

}