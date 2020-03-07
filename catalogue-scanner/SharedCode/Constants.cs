using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner
{
    internal static class Constants
    {
        public static class AppSettings
        {
            public const string ColesSaleFinderLocationId = "ColesSaleFinderLocationId";
        }

        public static class QueueNames
        {
            public const string CataloguesToDownload = "catalogue-scanner-catalogues-to-download";
            public const string DownloadedItems = "catalouge-scanner-downloaded-items";
            public const string MatchedItems = "catalogue-scanner-matched-items";
        }
    }
}
