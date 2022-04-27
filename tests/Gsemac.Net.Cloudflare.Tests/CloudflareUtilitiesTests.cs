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

            Assert.AreEqual(ProtectionType.AccessDenied, CloudflareUtilities.GetProtectionType(Properties.Resources.CloudflareAccessDenied));

        }
        [TestMethod]
        public void TestGetProtectionTypeWithCaptchaBypass() {

            Assert.AreEqual(ProtectionType.CaptchaBypass, CloudflareUtilities.GetProtectionType(Properties.Resources.CloudflareCaptcha));

        }
        [TestMethod]
        public void TestGetProtectionTypeWithCaptchaBypassWithoutMetaTag() {

            // The captcha page can be easily identified by the meta element with an id attribute set to "captcha-bypass".
            // However, this element is not always present, so it cannot be relied upon.

            Assert.AreEqual(ProtectionType.CaptchaBypass, CloudflareUtilities.GetProtectionType(Properties.Resources.CloudflareCaptchaWithoutMetaTag));

        }
        [TestMethod]
        public void TestGetProtectionTypeWithImUnderAttack() {

            Assert.AreEqual(ProtectionType.ImUnderAttack, CloudflareUtilities.GetProtectionType(Properties.Resources.CloudflareIuam));

        }

    }

}