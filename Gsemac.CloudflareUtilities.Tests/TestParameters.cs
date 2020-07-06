namespace Gsemac.CloudflareUtilities.Tests {

    public class TestParameters {

        // Public members

        public const string TestParametersFilePath = "test_parameters.json";

        public string Url { get; set; }
        public string FirefoxExecutablePath { get; set; }
        public string ChromeExecutablePath { get; set; }

        public static TestParameters GetTestParameters() {

            return Newtonsoft.Json.JsonConvert.DeserializeObject<TestParameters>(System.IO.File.ReadAllText(TestParametersFilePath));

        }

    }

}