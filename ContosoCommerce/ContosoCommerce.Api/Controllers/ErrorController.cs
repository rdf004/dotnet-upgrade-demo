using Microsoft.AspNetCore.Mvc;

namespace ContosoCommerce.Api.Controllers
{
    [ApiController]
    public class ErrorController
        : ControllerBase
    {
        [Route("/error")]
        [ApiExplorerSettings(
            IgnoreApi = true)]
        public IActionResult HandleError()
        {
            return Problem();
        }
    }
}
