using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkingAreasController : ControllerBase
    {
        private readonly WholesalerContext _context;

        public WorkingAreasController(WholesalerContext context)
        {
            _context = context;
        }
        // GET: api/WorkingAreas
        [HttpGet]
        public async Task<List<CityView>> Get()
        {
            return await _context.Cities.Select(city => new CityView
            {
                CityID = city.CityID,
                CityName = city.CityName,
                CountryID = city.CountryID ?? -1,
            }).ToListAsync();
        }

        // GET api/WorkingAreas/5
        [HttpGet("{id}")]
        public async Task<CityView> Get(int id)
        {
            return await _context.Cities.Select(city => new CityView
            {
                CityID = city.CityID,
                CityName = city.CityName,
                CountryID = city.CountryID ?? -1,
            }).FirstOrDefaultAsync( city => city.CityID == id);
        }

        // GET api/<WorkingAreas/
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CityView value)
        {
            if (string.IsNullOrEmpty(value.CityName))
            {
                return BadRequest("City name can not be null or empty");
            }
            if (await _context.Cities.FirstOrDefaultAsync(city => city.CityID == value.CityID) != null)
            {
                return BadRequest("City id already exist!");
            }
            _context.Cities.Add(new City { CityID = value.CityID, CityName = value.CityName, CountryID = value.CountryID, OperationalState = false });
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET api/WorkingAreas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CityView value)
        {
            if(string.IsNullOrEmpty(value.CityName))
            {
                return BadRequest("City name can not be null or empty");
            }
            if(_context.Cities.FirstOrDefaultAsync(city => city.CityID == id) == null)
            {
                return BadRequest("City id does not matching any city");
            }
            _context.Cities.Update(new City { CityID = value.CityID, CityName = value.CityName, CountryID = value.CountryID, OperationalState = false });
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET api/WorkingAreas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            City city = await _context.Cities.FirstOrDefaultAsync(model => model.CityID == id);
            if(city == null)
            {
                return NotFound("City not found!");
            }
            if(city.OperationalState)
            {
               return BadRequest("You can not delete operational cities");
            }
            return Ok();
        }
    }

    public class CityView
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
        public int CountryID { get; set; }
    }

}
