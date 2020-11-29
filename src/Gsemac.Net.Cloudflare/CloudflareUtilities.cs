namespace Gsemac.Net.Cloudflare {

    public static class CloudflareUtilities {

        public static ProtectionType GetProtectionType(string htmlDocument) {

            if (htmlDocument.Contains("cf-im-under-attack\">")) {

                return ProtectionType.ImUnderAttack;

            }
            else if (htmlDocument.Contains("id=\"captcha-bypass\">")) {


                return ProtectionType.CaptchaBypass;

            }
            else if (htmlDocument.Contains("cf-wrapper\">")) {

                if (htmlDocument.Contains("cf-captcha-container\">")) {

                    // This is the same challenge as the "One More Step" captcha.

                    return ProtectionType.CaptchaBypass;

                }
                else if (htmlDocument.Contains(") has banned your IP address (")) {

                    return ProtectionType.AccessDenied;

                }

            }

            return ProtectionType.None;

        }
        public static bool IsProtectionDetected(string htmlDocument) {

            return GetProtectionType(htmlDocument) != ProtectionType.None;

        }

    }

}