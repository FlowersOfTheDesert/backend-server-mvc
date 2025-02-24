using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_server_mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        [HttpPost("/create")]
        IActionResult SetSchedule()
        {
            return Ok();
        }


    }
}
