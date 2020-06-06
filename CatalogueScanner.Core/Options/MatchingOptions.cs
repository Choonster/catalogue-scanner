using CatalogueScanner.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.Core.Config
{
    public class MatchingOptions
    {
        public const string Matching = "Matching";

        public List<CatalogueItemMatchRule> Rules { get; } = new List<CatalogueItemMatchRule>();
    }
}
