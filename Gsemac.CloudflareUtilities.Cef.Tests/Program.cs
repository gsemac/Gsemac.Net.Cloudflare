using Gsemac.Assembly;
using System;
using System.Threading;

namespace Gsemac.CloudflareUtilities.Cef.Tests {
    class Program {

        static void Main(string[] args) {

            InitializeAssemblyResolver();

            string url = "https://censor.net.ua";

            IChallengeSolver challengeSolver = CreateChallengeSolver();
            challengeSolver.Log += (sender, e) => Console.Write(e.ToString());
            IChallengeResponse challengeResponse = challengeSolver.GetChallengeResponse(new Uri(url));
            
            Console.WriteLine(challengeResponse.UserAgent);

            Console.ReadKey();

        }

        // Private members

        private static void InitializeAssemblyResolver() {

            AnyCpuFileSystemAssemblyResolver assemblyResolver = new AnyCpuFileSystemAssemblyResolver();

            AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver.ResolveAssembly;

        }
        private static IChallengeSolver CreateChallengeSolver() {

            return new CefChallengeSolver(new CefChallengeSolverOptions() {
                BrowserSubprocessPath = System.IO.Path.Combine(EntryAssemblyInfo.GetDirectory(), Environment.Is64BitProcess ? "x64" : "x86", "CefSharp.BrowserSubprocess.exe"),
                Timeout = Timeout.Infinite
            });

        }

    }
}