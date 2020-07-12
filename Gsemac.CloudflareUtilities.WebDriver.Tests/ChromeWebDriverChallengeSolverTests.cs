using Gsemac.CloudflareUtilities.WebDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Gsemac.CloudflareUtilities.Tests {

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

            IChallengeSolver challengeSolver = CreateChallengeSolver();
            IChallengeResponse challengeResponse = challengeSolver.GetChallengeResponse(new Uri(testParameters.Url));

            Assert.IsTrue(challengeResponse.Success);

        }

        // Private members

        private IChallengeSolver CreateChallengeSolver() {

            TestParameters testParameters = TestParameters.GetTestParameters();

            WebDriverChallengeSolverOptions options = new WebDriverChallengeSolverOptions() {
                BrowserExecutablePath = testParameters.ChromeExecutablePath,
                Headless = true,
            };

            return new ChromeWebDriverChallengeSolver(options);

        }

    }
}