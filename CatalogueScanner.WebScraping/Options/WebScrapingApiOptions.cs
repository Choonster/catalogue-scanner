using System;
using System.ComponentModel.DataAnnotations;

namespace CatalogueScanner.WebScraping.Options
{
    public class WebScrapingApiOptions
    {
        public const string WebScrapingApi = "WebScrapingApi";

        [Required]
        public Uri BaseAddress { get; set; } = null!;

        [Required]
        public string Scope { get; set; } = null!;
    }
}
