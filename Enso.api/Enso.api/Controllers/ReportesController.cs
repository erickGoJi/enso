using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enso.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        // GET: api/Reportes
        [HttpGet]
        public IActionResult GetReportes()
        {
            return Ok("Reportes Controller");
        }
    }
}