namespace Gsemac.Net.Cloudflare.Cloudscraper {

    public class CloudscraperOptions :
        ICloudscraperOptions {

        public static CloudscraperOptions Default => new CloudscraperOptions();

        public string CloudscraperExecutablePath { get; set; }

    }

}