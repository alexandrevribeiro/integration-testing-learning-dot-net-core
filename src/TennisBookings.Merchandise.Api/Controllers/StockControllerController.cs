using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TennisBookings.Merchandise.Api.Models.Output;

namespace TennisBookings.Merchandise.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        [HttpGet("total")]
        public IActionResult Total()
        {
            return new JsonResult(new StockTotalOutputModel { StockItemTotal = 100 });
        }
    }
}
