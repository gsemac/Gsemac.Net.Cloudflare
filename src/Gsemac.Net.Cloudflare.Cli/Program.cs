using CommandLine;
using Gsemac.Net.Cloudflare.Iuam;
using Gsemac.Net.WebBrowsers;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace Gsemac.Net.Cloudflare.Cli {

    class Program {

        static void Main(string[] args) {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options => {

                // Build challenge solver options.

                IWebDriverIuamChallengeSolverOptions challengeSolverOptions = new WebDriverIuamChallengeSolverOptions();

                if (!string.IsNullOrEmpty(options.UserAgent))
                    challengeSolverOptions.UserAgent = options.UserAgent;

                challengeSolverOptions.Headless = options.Headless;

                // Build challenge solver.

                IIuamChallengeSolver solver = null;

                if (string.IsNullOrWhiteSpace(options.Solver) || options.Solver.Equals("webdriver")) {

                    if (options.Firefox)
                        challengeSolverOptions.BrowserExecutablePath = GetBrowserExecutablePath(WebBrowserId.Firefox);
                    else if (options.Chrome)
                        challengeSolverOptions.BrowserExecutablePath = GetBrowserExecutablePath(WebBrowserId.Chrome);
                    else
                        challengeSolverOptions.BrowserExecutablePath = GetBrowserExecutablePath();

                    solver = new WebDriverIuamChallengeSolver(challengeSolverOptions);

                }
                else if (options.Solver.Equals("manual")) {

                    IWebBrowserInfo webBrowserInfo = null;

                    if (options.Firefox)
                        webBrowserInfo = WebBrowserInfo.GetWebBrowserInfo(WebBrowserId.Firefox);
                    else if (options.Chrome)
                        webBrowserInfo = WebBrowserInfo.GetWebBrowserInfo(WebBrowserId.Chrome);

                    solver = new ManualWebBrowserIuamChallengeSolver(webBrowserInfo, new HttpWebRequestFactory(), challengeSolverOptions, () => true);

                }
                else if (options.Solver.Equals("flaresolverr")) {

                    solver = new FlareSolverrIuamChallengeSolver(challengeSolverOptions);

                }

                solver.Log += (sender, e) => Console.Error.Write(e.Message);

                // Get the challenge result.

                IIuamChallengeResponse challengeResponse = solver.GetChallengeResponse(new Uri(options.Url));
                string serializedChallengeResponse = JsonConvert.SerializeObject(challengeResponse, Formatting.Indented);

                Console.WriteLine(serializedChallengeResponse);

                Console.ReadKey();

            });
        }

        static string GetBrowserExecutablePath() {

            return WebBrowserInfo.GetWebBrowserInfo()
                  .Where(browser => browser.Id == WebBrowserId.Firefox || browser.Id == WebBrowserId.Chrome)
                  .Select(browser => browser.ExecutablePath)
                  .FirstOrDefault();

        }
        static string GetBrowserExecutablePath(WebBrowserId webBrowserId) {

            return WebBrowserInfo.GetWebBrowserInfo()
                  .Where(browser => browser.Id == webBrowserId)
                  .Select(browser => browser.ExecutablePath)
                  .FirstOrDefault();

        }

    }

}