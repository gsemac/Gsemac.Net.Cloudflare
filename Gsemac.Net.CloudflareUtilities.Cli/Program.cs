using CommandLine;
using Gsemac.Net.CloudflareUtilities.WebDriver;
using Gsemac.Net.WebBrowsers;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace Gsemac.Net.CloudflareUtilities.Cli {

    class Program {

        static void Main(string[] args) {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options => {

                // Build challenge solver options.

                IWebDriverChallengeSolverOptions challengeSolverOptions = new WebDriverChallengeSolverOptions();

                if (!string.IsNullOrEmpty(options.UserAgent))
                    challengeSolverOptions.UserAgent = options.UserAgent;

                challengeSolverOptions.Headless = options.Headless;

                // Build challenge solver.

                IChallengeSolver solver;

                if (options.Firefox)
                    challengeSolverOptions.BrowserExecutablePath = GetBrowserExecutablePath(WebBrowserId.Firefox);
                else if (options.Chrome)
                    challengeSolverOptions.BrowserExecutablePath = GetBrowserExecutablePath(WebBrowserId.Chrome);
                else
                    challengeSolverOptions.BrowserExecutablePath = GetBrowserExecutablePath();

                solver = new WebDriverChallengeSolver(challengeSolverOptions);

                solver.Log += (sender, e) => Console.Error.Write(e.Message);

                // Get the challenge result.

                IChallengeResponse challengeResponse = solver.GetChallengeResponse(new Uri(options.Url));
                string serializedChallengeResponse = JsonConvert.SerializeObject(challengeResponse, Formatting.Indented);

                Console.WriteLine(serializedChallengeResponse);

                Console.ReadKey();

            });
        }

        static string GetBrowserExecutablePath() {

            return WebBrowserUtilities.GetInstalledWebBrowsers()
                  .Where(browser => browser.Id == WebBrowserId.Firefox || browser.Id == WebBrowserId.Chrome)
                  .Select(browser => browser.ExecutablePath)
                  .FirstOrDefault();

        }
        static string GetBrowserExecutablePath(WebBrowserId webBrowserId) {

            return WebBrowserUtilities.GetInstalledWebBrowsers()
                  .Where(browser => browser.Id == webBrowserId)
                  .Select(browser => browser.ExecutablePath)
                  .FirstOrDefault();

        }

    }

}