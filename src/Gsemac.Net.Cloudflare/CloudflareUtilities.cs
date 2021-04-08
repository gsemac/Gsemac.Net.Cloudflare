using Gsemac.Net.JavaScript.Extensions;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Gsemac.Net.Cloudflare {

    public static class CloudflareUtilities {

        public static bool IsProtectionDetected(WebResponse webResponse) {

            if (webResponse is HttpWebResponse httpWebResponse) {

                bool isServiceUnavailable = httpWebResponse.StatusCode == HttpStatusCode.ServiceUnavailable;
                bool isCloudflareServer = httpWebResponse.Headers["Server"]?.Equals("cloudflare", StringComparison.OrdinalIgnoreCase) ?? false;

                return isServiceUnavailable && isCloudflareServer;

            }

            return false;


        }
        public static bool IsProtectionDetected(string htmlDocument) {

            return GetProtectionType(htmlDocument) != ProtectionType.None;

        }
        public static ProtectionType GetProtectionType(string htmlDocument) {

            Match protectionMatch = Regex.Match(htmlDocument,
                @"\bcf-(?:im-under-attack|cookie-error|captcha-container)|captcha-bypass|has banned your IP address\b");

            if (protectionMatch.Success) {

                switch (protectionMatch.Value) {

                    case "cf-im-under-attack":
                    case "cf-cookie-error":
                        return ProtectionType.ImUnderAttack;

                    case "cf-captcha-bypass":
                    case "cf-captcha-container":
                        return ProtectionType.CaptchaBypass;

                    case "has banned your IP address":
                        return ProtectionType.AccessDenied;

                }

            }

            return ProtectionType.None;

        }

        public static string DeobfuscateCfEmail(string cfEmail) {

            // for(e = '', r = '0x' + a.substr(0, 2) | 0, n = 2; a.length - n; n += 2) 
            //  e += ' % ' + ('0' + ('0x' + a.substr(n, 2) ^ r).toString(16)).slice(-2);

            string email = string.Empty;
            int r = Convert.ToInt32(cfEmail.Substring(0, 2), 16);

            for (int n = 2; cfEmail.Length - n > 0; n += 2)
                email += "%" + ("0" + (Convert.ToInt32(cfEmail.Substring(n, 2), 16) ^ r).ToString("X")).Slice(-2);

            return Uri.UnescapeDataString(email);

        }

    }

}