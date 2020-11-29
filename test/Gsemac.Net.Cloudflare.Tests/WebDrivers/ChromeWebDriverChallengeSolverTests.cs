using Gsemac.CloudflareUtilities.Tests;
using Gsemac.Net.Cloudflare.WebDrivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Gsemac.Net.Cloudflare.Tests {

    [TestClass]
    public class ChromeWebDriverChallengeSolverTests {

        // Public members

        [TestInitialize]
        public void Initialize() {

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        }

        [TestMethod]
        public void TestGetChallengeResponse() {

            TestParameters testParameters = TestParameters.GetTestParameters();

            IIuamChallengeSolver challengeSolver = CreateChallengeSolver();
            IIuamChallengeResponse challengeResponse = challengeSolver.GetChallengeResponse(new Uri(testParameters.Url));

            Assert.IsTrue(challengeResponse.Success);

        }

        // Private members

        private IIuamChallengeSolver CreateChallengeSolver() {

            TestParameters testParameters = TestParameters.GetTestParameters();

            WebDriverIuamChallengeSolverOptions options = new WebDriverIuamChallengeSolverOptions() {
                BrowserExecutablePath = testParameters.ChromeExecutablePath,
                Headless = true,
            };

            return new ChromeWebDriverIuamChallengeSolver(options);

        }

    }
}