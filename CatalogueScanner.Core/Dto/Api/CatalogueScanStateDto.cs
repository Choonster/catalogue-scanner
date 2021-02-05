using CatalogueScanner.Core.Functions.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CatalogueScanner.Core.Dto.Api
{
    public class CatalogueScanStateDto
    {
        /// <summary>
        /// The type of catalogue, this should be unique for each catalogue app/system; e.g. SaleFinder.
        /// </summary>
        public string CatalogueType { get; set; } = null!;

        /// <summary>
        /// The store that the catalogue belongs to.
        /// </summary>
        public string Store { get; set; } = null!;

        /// <summary>
        /// The ID of the catalogue. Each catalogue within a type/store should have a unique ID.
        /// </summary>
        public string CatalogueId { get; set; } = null!;

        /// <summary>
        /// The scan state of the catalogue.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ScanState ScanState { get; set; }
    }
}
