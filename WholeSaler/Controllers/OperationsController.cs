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
using WholeSaler.Utils;

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
        public async Task<IActionResult> Index(string sortOrder, string query, int? pageNumber, int? pageSize = 5)
        {
            if (!pageNumber.HasValue || pageNumber.Value < 1) pageNumber = 1;
            if (!pageSize.HasValue || pageSize.Value < 10) pageSize = 10;

            IQueryable<Operation> wholesellerContext = _context.Operations
                .Include(operation => operation.Basket)
                .Include(operation => operation.Owner)
                .Include(operation => operation.Location)
                .Include(operation => operation.Location.City)
                .Include(operation => operation.Location.City.Country);
            if (query != null)
            {
                wholesellerContext = wholesellerContext.Where(i => i.BasketID.ToString().Contains(query) | i.Location.City.CityName.Contains(query) | i.Location.City.Country.CountryName.Contains(query));
                TempData["Query"] = query;
            }
            if (sortOrder != null)
            {
                switch (sortOrder)
                {
                    default:
                    case "operation_owner":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.Owner.UserName);
                        break;
                    case "operation_value":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.OperationValue);
                        break;
                    case "operation_date":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.Date);
                        break;
                    case "operation_location":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.Location.Adress);
                        break;
                }
                TempData["CurrentFilter"] = sortOrder;
            }
            return View(await PaginatedList<Operation>.CreateAsync(wholesellerContext.AsNoTracking(), pageNumber ?? 1, pageSize.Value));
        }

        // GET: Operations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operation = await _context.Operations
                .Include(operation => operation.Basket)
                .Include(operation => operation.Owner)
                .Include(operation => operation.Location)
                .Include(operation => operation.Location.City)
                .Include(operation => operation.Location.City.Country)
                .FirstOrDefaultAsync(m => m.OperationID == id);
            if(operation == null)
            {
                return NotFound();
            }
            var items = await _context.BasketItems.Where(item => item.BasketID == operation.BasketID).Include(item => item.Item).Select(item => new BasketItem()
            {
                Amount = item.Amount,
                Item = new Item()
                {
                    ItemName = item.Item.ItemName,
                    ItemDesc = item.Item.ItemDesc,
                    ItemPrice = item.Item.ItemPrice
                }
            }).ToListAsync();
            if (operation == null)
            {
                return NotFound();
            }

            return View(new OperationDetailModel() { Operation = operation, Items = items });
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
                activeOperation.LastModifier = User.Identity.Name;
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

    public class OperationDetailModel
    {
        public Operation Operation { get; set; }
        public List<BasketItem> Items { get; set; }
    }
}
