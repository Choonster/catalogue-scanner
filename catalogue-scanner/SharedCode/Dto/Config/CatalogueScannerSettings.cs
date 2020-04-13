using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.Dto.Config
{
    public class CatalogueScannerSettings
    {
        public ColesSettings Coles { get; set; } = null!;
        public List<CatalogueItemMatchRule> Rules { get; } = new List<CatalogueItemMatchRule>();
        public EmailSettings Email { get; set; } = null!;
    }
}
