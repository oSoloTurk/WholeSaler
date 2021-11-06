using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Controllers
{
    public class OperationsController : Controller
    {
        private readonly WholesellerContext _context;

        public OperationsController(WholesellerContext context)
        {
            _context = context;
        }

        // GET: Operations
        public async Task<IActionResult> Index()
        {
            var wholesellerContext = _context.Operations.Include(operation => operation.Basket).Include(operation => operation.Date).Include(operation => operation.Location);
            return View(await wholesellerContext.ToListAsync());
        }

        // GET: Operations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations.Include(operation => operation.Basket).Include(operation => operation.Date).Include(operation => operation.Location)
                .FirstOrDefaultAsync(m => m.OperationID == id);
            if (operation == null)
            {
                return NotFound();
            }

            return View(operation);
        }

        // GET: Operations/SendVehicle
        public IActionResult SendVehicle()
        {
            ViewData["Vehicles"] = _context.Vehicles.ToList();
            ViewData["Operations"] = _context.Operations.Include(operation => operation.Owner).Include(operation => operation.Location).ToList();
            return View();
        }

        // POST: Operations/SendVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVehicle([Bind("OperationID,VehicleID")] Operation operation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(operation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
           return View(operation);
        }


        // GET: Operations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations.FindAsync(id);
            if (operation == null)
            {
                return NotFound();
            }
            ViewData["BasketID"] = new SelectList(_context.Baskets, "BasketID", "UserID", operation.BasketID);
            ViewData["DateID"] = new SelectList(_context.Dates, "DateID", "DateID", operation.DateID);
            ViewData["LocationID"] = new SelectList(_context.Locations.Include(loc => loc.City).Include(loc => loc.City.Country), "LocationID", "LocationOwnerID", operation.LocationID);
            return View(operation);
        }

        // POST: Operations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OperationID,DateID,BasketID,LocationID,OperationValue")] Operation operation)
        {
            if (id != operation.OperationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(operation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OperationExists(operation.OperationID))
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
            ViewData["BasketID"] = new SelectList(_context.Baskets, "BasketID", "UserID", operation.BasketID);
            ViewData["DateID"] = new SelectList(_context.Dates, "DateID", "DateID", operation.DateID);
            ViewData["LocationID"] = new SelectList(_context.Locations.Include(loc => loc.City).Include(loc => loc.City.Country), "LocationID", "LocationOwnerID", operation.LocationID);
            return View(operation);
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
                .Include(o => o.Date)
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
