using CatalogueScanner.Core.Dto.Api;
using CatalogueScanner.Core.Dto.Api.Request;
using CatalogueScanner.Core.Dto.Api.Result;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatalogueScanner.ConfigurationUI.Service
{
    public class CatalogueScanStateService : BaseApiService
    {
        public CatalogueScanStateService(HttpClient httpClient, TokenProvider tokenProvider) : base(httpClient, tokenProvider)
        {
        }

        public async Task<ListEntityResult<CatalogueScanStateDto>?> ListCatalogueScanStatesAsync(ListEntityRequest listEntityRequest) =>
            await PostAsync<ListEntityRequest, ListEntityResult<CatalogueScanStateDto>>("List", listEntityRequest).ConfigureAwait(false);

        public async Task UpdateCatalogueScanStateAsync(CatalogueScanStateDto dto) =>
            await PostAsync<CatalogueScanStateDto, object>("Update", dto).ConfigureAwait(false);
    }
}
