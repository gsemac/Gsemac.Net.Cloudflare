namespace Gsemac.Cloudflare.CFClearanceScraper {

    internal interface ICFClearanceScraperService {

        ICfClearanceScraperResponse GetResponse(ICfClearanceScraperRequest request);

    }

}