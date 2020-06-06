using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.Core
{
    public class CoreConstants
    {
        public static class FunctionNames
        {


            /// <summary>
            /// The name of the <see cref="Function.FilterCatalogueItem.FilterCatalogueItem"/> activity function.
            /// </summary>
            public const string FilterCatalogueItem = "FilterCatalogueItem";

            /// <summary>
            /// The name of the <see cref="CatalogueScanner.SendCatalogueDigestEmail"/> activity function.
            /// </summary>
            public const string SendCatalogueDigestEmail = "SendCatalogueDigestEmail";
        }

        public static class QueueNames
        {
            public const string SaleFinderCataloguesToScan = "catalogue-scanner-sale-finder-catalogues-to-scan";
        }

        public static class AppSettingNames
        {
            /// <summary>
            /// The CRON expression used for the timer triggers of the Check[Store]Catalogue functions.
            /// </summary>
            public const string CheckCatalogueFunctionCronExpression = "CheckCatalogueFunctionCronExpression";
        }
    }
}
