# Gsemac.Net.Cloudflare
[![NuGet](https://img.shields.io/nuget/v/Gsemac.Net.Cloudflare.svg)](https://www.nuget.org/packages/Gsemac.Net.Cloudflare/) 
[![NuGet](https://img.shields.io/nuget/dt/Gsemac.Net.Cloudflare)](https://www.nuget.org/packages/Gsemac.Net.Cloudflare/)

Gsemac.Net.Cloudflare is a library for interacting with Cloudflare-protected websites designed to be compatible with .NET Framework 4.0 and later.

Classes are provided for passing `HttpWebRequest`-based requests through [cloudscraper](https://github.com/VeNoMouS/cloudscraper), [FlareSolverr](https://github.com/FlareSolverr/FlareSolverr), and Selenium.

## Usage

The bypass is implemented by use of the delegating handler implementation from [Gsemac.Net](https://github.com/gsemac/Gsemac.Common). To use FlareSolverr, first register the `FlareSolverrService` and `FlareSolverrChallengeHandler` services. `WebClientFactory` will use the handler to pass requests through FlareSolverr.

```csharp
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
            Console.WriteLine(webClient.DownloadString("https://example.com/"));

    }

}
```