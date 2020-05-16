using System.Collections.Generic;

namespace CatalogueScanner.Dto.Config
{
    public class CatalogueScannerSettings
    {
        public ColesSettings Coles { get; set; } = null!;
        public WoolworthsSettings Woolworths { get; set; } = null!;
        public List<CatalogueItemMatchRule> Rules { get; } = new List<CatalogueItemMatchRule>();
        public EmailSettings Email { get; set; } = null!;
    }
}
