using Gsemac.IO.Logging;
using Gsemac.Net;
using Gsemac.Net.Cloudflare.FlareSolverr;
using Gsemac.Net.Http;
using Gsemac.Polyfills.Microsoft.Extensions.DependencyInjection;

namespace TestConsole {

    internal class Program {

        static ServiceProvider CreateServiceProvider() {

            return new ServiceCollection()
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<IWebClientFactory, WebClientFactory>()
                .AddSingleton<IFlareSolverrService, FlareSolverrService>()
                .AddSingleton<WebRequestHandler, FlareSolverrChallengeHandler>()
                .BuildServiceProvider();

        }

        static void Main(string[] args) {

            using (ServiceProvider serviceProvider = CreateServiceProvider()) {

                IWebClientFactory webClientFactory = serviceProvider.GetRequiredService<IWebClientFactory>();

                using (IWebClient webClient = webClientFactory.Create())
                    Console.WriteLine(webClient.DownloadString("https://manhuaplus.com/manga/tales-of-demons-and-gods/"));

                using (IWebClient webClient = webClientFactory.Create())
                    Console.WriteLine(webClient.UploadString("https://manhuaplus.com/manga/tales-of-demons-and-gods/ajax/chapters/", ""));

            }

        }

    }

}