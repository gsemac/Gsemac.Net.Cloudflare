using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gsemac.Net.Cloudflare.Tests {

    [TestClass]
    public class CloudflareUtilitiesTests {

        // DeobfuscateCfEmail

        [TestMethod]
        public void TestDeobfuscateCfEmail() {

            Assert.AreEqual("me@usamaejaz.com", CloudflareUtilities.DeobfuscateCfEmail("543931142127353935313e352e7a373b39"));

        }

        // GetProtectionType

        [TestMethod]
        public void TestGetProtectionTypeWithAccessDenied() {

            Assert.AreEqual(ProtectionType.AccessDenied, CloudflareUtilities.GetProtectionType(Properties.Resources.cf_cookie_error));

        }
        [TestMethod]
        public void TestGetProtectionTypeWithCaptchaBypass() {

            Assert.AreEqual(ProtectionType.CaptchaBypass, CloudflareUtilities.GetProtectionType(Properties.Resources.captcha_bypass));

        }
        [TestMethod]
        public void TestGetProtectionTypeWithImUnderAttack() {

            Assert.AreEqual(ProtectionType.ImUnderAttack, CloudflareUtilities.GetProtectionType(Properties.Resources.im_under_attack));

        }

    }

}