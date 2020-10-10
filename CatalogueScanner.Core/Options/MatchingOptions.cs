using CatalogueScanner.Core.MatchRule;
using System.Collections.Generic;

namespace CatalogueScanner.Core.Options
{
    public class MatchingOptions
    {
        public const string Matching = "Matching";

        public List<ICatalogueItemMatchRule> Rules { get; } = new List<ICatalogueItemMatchRule>();
    }
}
