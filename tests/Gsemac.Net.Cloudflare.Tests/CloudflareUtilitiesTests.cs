using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gsemac.Net.Cloudflare.Tests {

    [TestClass]
    public class CloudflareUtilitiesTests {

        // DeobfuscateCfEmail

        [TestMethod]
        public void TestDeobfuscateCfEmail() {

            Assert.AreEqual("me@usamaejaz.com", CloudflareUtilities.DeobfuscateCfEmail("543931142127353935313e352e7a373b39"));

        }

    }

}