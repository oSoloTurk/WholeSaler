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
    public class OperationsController : Controller
    {
        private readonly WholesalerContext _context;

        public OperationsController(WholesalerContext context)
        {
            _context = context;
        }

        // GET: Operations
        public async Task<IActionResult> Index()
        {
            var wholesellerContext = _context.Operations
                .Include(operation => operation.Basket)
                .Include(operation => operation.Location)
                .Include(operation => operation.Location.City)
                .Include(operation => operation.Location.City.Country);
            return View(await wholesellerContext.ToListAsync());
        }

        // GET: Operations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations.Include(operation => operation.Basket).Include(operation => operation.Location)
                .FirstOrDefaultAsync(m => m.OperationID == id);
            if (operation == null)
            {
                return NotFound();
            }

            return View(operation);
        }

        // GET: Operations/SendVehicle
        public async Task<IActionResult> SendVehicle()
        {
            ViewBag.vehiclesInTheOperation = await _context.Operations.Distinct().Select(operation => operation.VehicleID).ToListAsync();
            ViewData["Vehicles"] = await _context.Vehicles.ToListAsync();
            ViewData["Operations"] = await _context.Operations.Where(operation => operation.VehicleID == null).Include(operation => operation.Owner).Include(operation => operation.Location).ToListAsync();
            return View();
        }

        // POST: Operations/SendVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVehicle([Bind("OperationID,VehicleID")] Operation operation)
        {
            if (ModelState.IsValid)
            {
                var activeOperation = await _context.Operations.FindAsync(operation.OperationID);
                activeOperation.VehicleID = operation.VehicleID;
                _context.Update(activeOperation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
           return View();
        }

        // GET: Operations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations
                .Include(o => o.Basket)
                .Include(o => o.Location)
                .FirstOrDefaultAsync(m => m.OperationID == id);
            if (operation == null)
            {
                return NotFound();
            }

            return View(operation);
        }

        // POST: Operations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var operation = await _context.Operations.FindAsync(id);
            _context.Operations.Remove(operation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OperationExists(int id)
        {
            return _context.Operations.Any(e => e.OperationID == id);
        }
    }
}
