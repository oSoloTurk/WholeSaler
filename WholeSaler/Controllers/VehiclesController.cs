using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WholeSaler.Models;
using WholeSaler.Data;
using Microsoft.AspNetCore.Authorization;
using WholeSaler.Utils;
using WholeSaler.Services;
using Microsoft.Extensions.Localization;

namespace WholeSaler.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin, Employee")]
    public class VehiclesController : Controller
    {
        private readonly WholesalerContext _context;
        private readonly AlertService _alertService;
        private readonly IStringLocalizer<VehiclesController> _localizer;

        public VehiclesController(WholesalerContext context, IStringLocalizer<VehiclesController> localizer)
        {
            _context = context; 
            _alertService = new AlertService(_context);
            _localizer = localizer;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index(string sortOrder, string query, int? pageNumber, int? pageSize = 5)
        {
            if (!pageNumber.HasValue || pageNumber.Value < 1) pageNumber = 1;
            if (!pageSize.HasValue || pageSize.Value < 10) pageSize = 10;

            IQueryable<Vehicle> wholesellerContext = _context.Vehicles;
            if (query != null)
            {
                wholesellerContext = wholesellerContext.Where(l => l.VehicleID.ToString().Contains(query) || l.VehicleName.Contains(query) || l.VehiclePlate.Contains(query));
                TempData["Query"] = query;
            }
            if (sortOrder != null)
            {
                switch (sortOrder)
                {
                    default:
                    case "vehicle_name":
                        wholesellerContext = wholesellerContext.OrderBy(vehicle => vehicle.VehicleName);
                        break;
                    case "vehicle_plate":
                        wholesellerContext = wholesellerContext.OrderBy(vehicle => vehicle.VehiclePlate);
                        break;
                }
                TempData["CurrentFilter"] = sortOrder;
            }
            ViewBag.vehiclesInTheOperation = await _context.Operations.Distinct().Select(operation => operation.VehicleID).ToListAsync();
            return View(await PaginatedList<Vehicle>.CreateAsync(wholesellerContext.AsNoTracking(), pageNumber ?? 1, pageSize.Value));
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(m => m.VehicleID == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleID,VehicleName,VehiclePlate")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                vehicle.LastModifier = User.Identity.Name;
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VehicleID,VehicleName,VehiclePlate")] Vehicle vehicle)
        {
            if (id != vehicle.VehicleID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vehicle.LastModifier = User.Identity.Name;
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.VehicleID))
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
            return View(vehicle);
        }

        public async Task<IActionResult> EndOperation(int? id)
        {
            if (id != null)
            {
                var operation = await _context.Operations.FirstOrDefaultAsync(operation => operation.VehicleID == id);
                operation.VehicleID = null;
                _context.Operations.Update(operation);
                await _alertService.SendAlert(operation.OwnerID, _localizer["We checked your take the orders and changed operation status to completed"], Url.Action("UserBoard", "Dashboard"));
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(m => m.VehicleID == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.VehicleID == id);
        }
    }
}
