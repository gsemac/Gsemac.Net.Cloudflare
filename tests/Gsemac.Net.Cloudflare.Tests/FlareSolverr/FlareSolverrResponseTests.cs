using Gsemac.Net.Cloudflare.Tests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Gsemac.Net.Cloudflare.FlareSolverr.Tests {

    [TestClass]
    public class FlareSolverrResponseTests {

        [TestMethod]
        public void TestDeserializeFlareSolverrResponseWithCookiesWithEmptyStringForNameDoesNotThrow() {

            string responseJson = Resources.ResponseWithCookiesWithEmptyStringForName;
            IFlareSolverrResponse flareSolverrResponse = JsonConvert.DeserializeObject<FlareSolverrResponse>(responseJson);

            Assert.AreEqual(1, flareSolverrResponse.Solution.Cookies.Count);

        }

    }

}