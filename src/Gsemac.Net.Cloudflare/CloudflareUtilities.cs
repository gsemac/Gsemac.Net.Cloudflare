using Gsemac.Net.Http;
using Gsemac.Net.JavaScript.Extensions;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Gsemac.Net.Cloudflare {

    public static class CloudflareUtilities {

        // Public members

        public static bool IsProtectionDetected(WebResponse webResponse) {

            if (webResponse is null)
                throw new ArgumentNullException(nameof(webResponse));

            switch (webResponse) {

                case HttpWebResponse httpWebResponse:
                    return IsProtectionDetected(httpWebResponse);

                case IHttpWebResponse iHttpWebResponse:
                    return IsProtectionDetected(iHttpWebResponse);

                default:
                    return false;

            }

        }
        public static bool IsProtectionDetected(IHttpWebResponse webResponse) {

            if (webResponse is null)
                throw new ArgumentNullException(nameof(webResponse));

            // We usually get a 503, but sometimes Cloudflare will complain about cookies not being enabled and returned a 403 instead.

            bool isServiceUnavailable = webResponse.StatusCode == HttpStatusCode.ServiceUnavailable ||
                webResponse.StatusCode == HttpStatusCode.Forbidden;

            // Check the server header, which will indicate the protection service being used. 

            string serverHeader = webResponse.Headers["Server"] ?? string.Empty;

            bool hasProtectionServerHeader = serverHeader.Equals("cloudflare", StringComparison.OrdinalIgnoreCase) ||
                serverHeader.Equals("ddos-guard", StringComparison.OrdinalIgnoreCase);

            return isServiceUnavailable && hasProtectionServerHeader;

        }
        public static bool IsProtectionDetected(string htmlDocument) {

            return GetProtectionType(htmlDocument) != ProtectionType.None;

        }

        public static ProtectionType GetProtectionType(WebResponse webResponse) {

            if (webResponse is null)
                throw new ArgumentNullException(nameof(webResponse));

            switch (webResponse) {

                case HttpWebResponse httpWebResponse:
                    return GetProtectionType(httpWebResponse);

                case IHttpWebResponse iHttpWebResponse:
                    return GetProtectionType(iHttpWebResponse);

                default:
                    return ProtectionType.None;

            }

        }
        public static ProtectionType GetProtectionType(IHttpWebResponse webResponse) {

            if (webResponse is null)
                throw new ArgumentNullException(nameof(webResponse));

            if (!IsProtectionDetected(webResponse))
                return ProtectionType.None;

            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                return GetProtectionType(sr.ReadToEnd());

        }
        public static ProtectionType GetProtectionType(string htmlDocument) {

            if (string.IsNullOrWhiteSpace(htmlDocument))
                return ProtectionType.None;

            Match protectionMatch = Regex.Match(htmlDocument,
                @"\bcf-(?:im-under-attack|captcha-container)|captcha-bypass|<title>Access denied|has banned your IP address|<title>DDoS-Guard\b",
                RegexOptions.IgnoreCase);

            if (protectionMatch.Success) {

                switch (protectionMatch.Value) {

                    case "cf-im-under-attack":
                        return ProtectionType.ImUnderAttack;

                    case "captcha-bypass":
                    case "cf-captcha-container":
                        return ProtectionType.CaptchaBypass;

                    case "<title>Access denied":
                    case "has banned your IP address":
                        return ProtectionType.AccessDenied;

                    case "<title>DDoS-Guard":
                        return ProtectionType.DDosGuard;

                }

            }

            return ProtectionType.None;

        }

        public static bool IsCloudflareCookie(Cookie cookie) {

            if (cookie is null)
                throw new ArgumentNullException(nameof(cookie));

            return cookie.Name.StartsWith("cf_") ||
                cookie.Name.StartsWith("_cf") ||
                cookie.Name.StartsWith("__cf");

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

        // Private members

        private static bool IsProtectionDetected(HttpWebResponse webResponse) {

            if (webResponse is null)
                throw new ArgumentNullException(nameof(webResponse));

            return IsProtectionDetected((IHttpWebResponse)new HttpWebResponseAdapter(webResponse));

        }
        private static ProtectionType GetProtectionType(HttpWebResponse webResponse) {

            if (webResponse is null)
                throw new ArgumentNullException(nameof(webResponse));

            return GetProtectionType((IHttpWebResponse)new HttpWebResponseAdapter(webResponse));

        }

    }

}