using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.SharedCode.Dto.StorageEntity
{
    /// <summary>
    /// Represents a SaleFinder catalogue to be downloaded.
    /// </summary>
    public class SaleFinderCatalogueDownloadInformation
    {
        /// <summary>
        /// The ID of the SaleFinder catalogue.
        /// </summary>
        public int SaleId { get; set; }
    }
}
