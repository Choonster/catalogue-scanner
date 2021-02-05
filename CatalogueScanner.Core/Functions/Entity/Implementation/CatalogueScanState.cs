using CatalogueScanner.Core.Dto.EntityKey;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Functions.Entity.Implementation
{
    /// <summary>
    /// Durable entity that stores the scan state of an individual catalogue, so that each catalogue is only scanned once.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CatalogueScanState : ICatalogueScanState
    {
        /// <summary>
        /// The type of catalogue, this should be unique for each catalogue app/system; e.g. SaleFinder.
        /// </summary>
        [JsonProperty("catalogueType")]
        public string CatalogueType { get; }

        /// <summary>
        /// The store that the catalogue belongs to.
        /// </summary>
        [JsonProperty("store")]
        public string Store { get; }

        /// <summary>
        /// The ID of the catalogue. Each catalogue within a type/store should have a unique ID.
        /// </summary>
        [JsonProperty("catalogueId")]
        public string CatalogueId { get; }

        /// <summary>
        /// The scan state of the catalogue.
        /// </summary>
        [JsonProperty("scanState")]
        public ScanState ScanState { get; set; } = ScanState.NotStarted;

        public CatalogueScanState(string catalogueType, string store, string catalogueId)
        {
            CatalogueType = catalogueType;
            Store = store;
            CatalogueId = catalogueId;
        }

        public Task<ScanState> GetState()
        {
            return Task.FromResult(ScanState);
        }

        public Task UpdateState(ScanState scanState)
        {
            ScanState = scanState;
            return Task.CompletedTask;
        }

        [FunctionName(nameof(CatalogueScanState))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            #region null checks
            if (ctx is null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            #endregion

            var key = CatalogueScanStateKey.FromString(ctx.EntityKey);

            return ctx.DispatchAsync<CatalogueScanState>(key.CatalogueType, key.Store, key.CatalogueId);
        }
    }
}
