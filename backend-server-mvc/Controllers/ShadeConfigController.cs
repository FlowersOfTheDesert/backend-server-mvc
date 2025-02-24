using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_server_mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShadeConfigController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateConfig()
        {
            return Ok();
        }
    }
}
