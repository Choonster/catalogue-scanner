using System;

namespace CatalogueScanner.WebScraping.Common.Dto.ColesOnline
{
    public class ColesOnlineSpecialsResult
    {
        public Uri ProductUrlTemplate { get; set; }
        public ColrsCatalogEntryList Data { get; set; }

        public ColesOnlineSpecialsResult(Uri productUrlTemplate, ColrsCatalogEntryList data)
        {
            ProductUrlTemplate = productUrlTemplate;
            Data = data;
        }
    }
}
