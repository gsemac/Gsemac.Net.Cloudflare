using Gsemac.Assembly;
using System;

namespace Gsemac.CloudflareUtilities.Cef.Tests {
    class Program {

        static void Main(string[] args) {

            InitializeAssemblyResolver();

            string url = "";

            IChallengeSolver challengeSolver = CreateChallengeSolver();
            IChallengeResponse challengeResponse = challengeSolver.GetChallengeResponse(url);

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
                Timeout = 60000
            });

        }

    }
}