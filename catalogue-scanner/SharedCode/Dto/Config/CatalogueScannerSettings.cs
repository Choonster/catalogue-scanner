using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.Dto.Config
{
    public class CatalogueScannerSettings
    {
        public ColesSettings Coles { get; set; }
        public List<CatalogueItemMatchRule> Rules { get; } = new List<CatalogueItemMatchRule>();
    }
}
