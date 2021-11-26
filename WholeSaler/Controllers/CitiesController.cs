using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class CitiesController : Controller
    {
        private readonly WholesalerContext _context;

        public CitiesController(WholesalerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, string query)
        {
            IQueryable<City> wholesellerContext = _context.Cities.Include(city => city.Country);
            if (query != null)
            {
                wholesellerContext = wholesellerContext.Where(c => c.CityID.ToString().Contains(query) || c.CityName.Contains(query) || c.Country.CountryName.Contains(query));
                TempData["Query"] = query;
            }
            if (sortOrder != null)
            {
                switch (sortOrder)
                {
                    default:
                    case "city_name":
                        wholesellerContext = wholesellerContext.OrderBy(city => city.CityName);
                        break;
                    case "country_name":
                        wholesellerContext = wholesellerContext.OrderBy(city => city.Country.CountryName);
                        break;
                }
                TempData["CurrentFilter"] = sortOrder;
            }
            return View(await wholesellerContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["Countries"] = new SelectList(_context.Countries, "CountryID", "CountryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CountryID,CityName")] City city)
        {
            if (ModelState.IsValid)
            {
                _context.Add(city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            ViewData["Countries"] = new SelectList(_context.Countries, "CountryID", "CountryName");
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CityID,CountryID,CityName")] City city)
        {
            if (id != city.CityID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(city);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CityExists(city.CityID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }

        // GET: Cities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FindAsync(id);

            if (city == null)
            {
                return NotFound();
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.CityID == id);
        }
    }
}
