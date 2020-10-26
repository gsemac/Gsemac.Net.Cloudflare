using CommandLine;
using Gsemac.Net.CloudflareUtilities.WebDriver;
using Newtonsoft.Json;
using System;

namespace Gsemac.Net.CloudflareUtilities.Cli {

    class Program {

        static void Main(string[] args) {

            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options => {

                // Build challenge solver options.

                IWebDriverChallengeSolverOptions challengeSolverOptions = new WebDriverChallengeSolverOptions();

                if (!string.IsNullOrEmpty(options.UserAgent))
                    challengeSolverOptions.UserAgent = options.UserAgent;

                challengeSolverOptions.Headless = options.Headless;

                // Build challenge solver.

                IChallengeSolver solver;

                if (options.Firefox)
                    solver = new FirefoxWebDriverChallengeSolver(challengeSolverOptions);
                else if (options.Chrome)
                    solver = new ChromeWebDriverChallengeSolver(challengeSolverOptions);
                else
                    solver = new WebDriverChallengeSolver(challengeSolverOptions);

                solver.Log += (sender, e) => Console.Error.WriteLine(e.Message);

                // Get the challenge result.

                IChallengeResponse challengeResponse = solver.GetChallengeResponse(new Uri(options.Url));
                string serializedChallengeResponse = JsonConvert.SerializeObject(challengeResponse);

                Console.WriteLine(serializedChallengeResponse);

            });
        }

    }

}