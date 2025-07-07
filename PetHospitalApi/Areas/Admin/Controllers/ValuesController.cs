using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PetHospitalApi.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "This is a test response from the Admin area." });
        }
    }
}
