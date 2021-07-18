using CatalogueScanner.WebScraping.API.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CatalogueScanner.ColesOnline.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ColesOnlineController : ControllerBase
    {
        private readonly ColesOnlineService colesPlaywrightTest;

        public ColesOnlineController(ColesOnlineService colesPlaywrightTest)
        {
            this.colesPlaywrightTest = colesPlaywrightTest;
        }

        [HttpGet]
        [Route("specials")]
        public async Task<IActionResult> GetSpecialsAsync()
        {
            dynamic productData = await colesPlaywrightTest.GetSpecialsAsync().ConfigureAwait(true);

            return Ok(productData);
        }
    }
}
