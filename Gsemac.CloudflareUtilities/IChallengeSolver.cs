using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace Gsemac.CloudflareUtilities {

    public interface IChallengeSolver {

        IChallengeResponse GetChallengeResponse(string url);

    }

}