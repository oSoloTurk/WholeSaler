using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WholeSaler.Data;
using WholeSaler.Models;
using WholeSaler.Utils;

namespace WholeSaler.Controllers
{
    [Authorize(Roles = "SuperAdmin, Customer")]
    public class LocationsController : Controller
    {
        private readonly WholesalerContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public LocationsController(WholesalerContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Locations
        public async Task<IActionResult> Index(string sortOrder, string query, int? pageNumber, int? pageSize = 10)
        {
            if (!pageNumber.HasValue || pageNumber.Value < 1) pageNumber = 1;
            if (!pageSize.HasValue || pageSize.Value < 10) pageSize = 10;

            IQueryable<Location> wholesellerContext = _context.Locations.Include(l => l.City).Include(l => l.LocationOwner);
            if (query != null)
            {
                wholesellerContext = wholesellerContext.Where(i => i.Adress.Contains(query) | i.City.CityName.Contains(query));
                TempData["Query"] = query;
            }
            if (sortOrder != null)
            {
                switch (sortOrder)
                {
                    default:
                    case "location_adress":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.Adress);
                        break;
                    case "location_cityname":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.City.CityName);
                        break;
                    case "location_owner":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.LocationOwner);
                        break;
                }
                TempData["CurrentFilter"] = sortOrder;
            }
            return View(await PaginatedList<Location>.CreateAsync(wholesellerContext.AsNoTracking(), pageNumber ?? 1, pageSize.Value));
        }

        [Authorize]
        public async Task<IActionResult> OwnLocations(string sortOrder, string query, int? pageNumber, int? pageSize = 5)
        {
            if (!pageNumber.HasValue || pageNumber.Value < 1) pageNumber = 1;
            if (!pageSize.HasValue || pageSize.Value < 10) pageSize = 10;

            var wholesellerContext = _context.Locations.Include(l => l.City).Include(l => l.LocationOwner).Where(l => l.LocationOwnerID == _userManager.GetUserId(User));
            if (query != null)
            {
                wholesellerContext = wholesellerContext.Where(l => l.LocationID.ToString().Contains(query) | l.LocationOwner.UserName.Contains(query));
                TempData["Query"] = query;
            }
            if (sortOrder != null)
            {
                switch (sortOrder)
                {
                    default:
                    case "adress":
                        wholesellerContext = wholesellerContext.OrderBy(loc => loc.Adress);
                        break;
                    case "city_name":
                        wholesellerContext = wholesellerContext.OrderBy(loc => loc.City.CityName);
                        break;
                }
                TempData["CurrentFilter"] = sortOrder;
            }
            return View(await PaginatedList<Location>.CreateAsync(wholesellerContext.AsNoTracking(), pageNumber ?? 1, pageSize.Value));
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .Include(l => l.City)
                .Include(l => l.LocationOwner)
                .FirstOrDefaultAsync(m => m.LocationID == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Locations/Create
        public IActionResult Create(string? returnUrl)
        {
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName");
            ViewData["LocationOwnerID"] = new SelectList(_context.Set<User>(), "Id", "Id");
            TempData["returnUrl"] = returnUrl;
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? returnUrl, [Bind("Adress,CityID")] Location location)
        {
            if (ModelState.IsValid)
            {
                location.LastModifier = User.Identity.Name;
                location.LocationOwnerID = _userManager.GetUserId(User);
                _context.Add(location);
                await _context.SaveChangesAsync();
                if (returnUrl != null)
                {
                    return LocalRedirect(returnUrl);
                }
            }
            return View("Index");
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", location.CityID);
            ViewData["LocationOwnerID"] = new SelectList(_context.Set<User>(), "Id", "Id", location.LocationOwnerID);
            return View(location);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LocationID,LocationOwnerID,Adress,CityID")] Location location)
        {
            if (id != location.LocationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    location.LastModifier = User.Identity.Name;
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.LocationID))
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
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", location.CityID);
            ViewData["LocationOwnerID"] = new SelectList(_context.Set<User>(), "Id", "Id", location.LocationOwnerID);
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .Include(l => l.City)
                .Include(l => l.LocationOwner)
                .FirstOrDefaultAsync(m => m.LocationID == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.LocationID == id);
        }
    }
}
