using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enso.api.Utility;
using Enso.dal.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Enso.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferralsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly Db_EnsoContext _context;

        public ReferralsController(IConfiguration config, Db_EnsoContext context)
        {
            this._config = config;
            this._context = context;
        }

        // GET: api/Referrals
        [HttpGet]
        public IActionResult Get(string kolid)
        {            
            Client client = _context.Clients.FirstOrDefault(c => c.Kolid == kolid);    
            
            if (client == null) { return Ok(new { result = "Error"}); }

            return Ok(new { result = "Success", cliente = client });            
        }

        // GET: api/Referrals/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Referrals
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        // PUT: api/Referrals/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}
