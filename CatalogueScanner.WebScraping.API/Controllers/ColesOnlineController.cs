using CatalogueScanner.WebScraping.API.Service;
using CatalogueScanner.WebScraping.Common.Dto.ColesOnline;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CatalogueScanner.ColesOnline.API.Controllers
{
    [Route("coles-online")]
    [ApiController]
    public class ColesOnlineController : ControllerBase
    {
        private readonly ColesOnlineService colesOnlineService;

        public ColesOnlineController(ColesOnlineService colesOnlineService)
        {
            this.colesOnlineService = colesOnlineService;
        }

        [HttpGet]
        [Route("specials")]
        public async Task<IActionResult> GetSpecialsAsync()
        {
            var productUrTemplate = colesOnlineService.ProductUrlTemplate;
            var productData = await colesOnlineService.GetSpecialsAsync().ConfigureAwait(true);

            return Ok(new ColesOnlineSpecialsResult(productUrTemplate, productData));
        }
    }
}
