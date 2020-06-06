using System.Collections.Generic;

namespace CatalogueScanner.Core.Dto.Config
{
    public class CatalogueScannerSettings
    {
        public List<CatalogueItemMatchRule> Rules { get; } = new List<CatalogueItemMatchRule>();
        public EmailSettings Email { get; set; } = null!;
    }
}
