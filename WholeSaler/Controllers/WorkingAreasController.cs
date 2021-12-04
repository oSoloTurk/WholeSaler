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
                CityID = city.CityID.ToString(),
                CityName = city.CityName,
                CountryID = city.CountryID != null ? city.CountryID.ToString() : "-1",
        }).ToListAsync();
        }

        // GET api/WorkingAreas/5
        [HttpGet("{id}")]
        public async Task<CityView> Get(int id)
        {
            return await _context.Cities.Select(city => new CityView
            {
                CityID = city.CityID.ToString(),
                CityName = city.CityName,
                CountryID = city.CountryID != null ? city.CountryID.ToString() : "-1",
            }).FirstOrDefaultAsync( city => Int32.Parse(city.CityID) == id);
        }

        // POST api/WorkingAreas
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CityView value)
        {
            int cityId = Int32.Parse(value.CityID);
            int countryId = Int32.Parse(value.CountryID);
            if (value.CityName != "" && value.CountryName != "")
            {
                _context.Countries.Add(new Country { CountryName = value.CountryName });
                await _context.SaveChangesAsync();
                int newId = (await _context.Countries.Where(country => country.CountryName == value.CountryName).FirstOrDefaultAsync()).CountryID;
                _context.Cities.Add(new City { CityID = newId, CityName = value.CityName, CountryID = countryId, OperationalState = false });
            }
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
            int cityId = Int32.Parse(value.CityID);
            int countryId = Int32.Parse(value.CountryID);
            if(value.CityName == "")
            {
                _context.Cities.Update(new City { CityID = cityId, CityName = value.CityName, CountryID = countryId, OperationalState = false });
            }
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
        public string CityID { get; set; }
        public string CityName { get; set; }
        public string CountryID { get; set; }
        public string CountryName { get; set; }
    }
}
