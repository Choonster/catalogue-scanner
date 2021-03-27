using CatalogueScanner.Core.Dto.Api;
using System;

namespace CatalogueScanner.ConfigurationUI.ViewModel
{
    public class CatalogueScanStateViewModel : CatalogueScanStateDto
    {
        /// <summary>
        /// The local time of the last change to this scan state record.
        /// </summary>
        public DateTime LastOperationLocalTime { get; set; }
    }
}
