using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;
using WholeSaler.Utils;

namespace WholeSaler.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin, Customer")]
    public class DashboardController : Controller
    {
        private readonly WholesalerContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(WholesalerContext context, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            if(User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("AdminBoard");
            }
            if(User.IsInRole("Customer"))
            {
                return RedirectToAction("UserBoard");
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UserBoard(int? pageNumber, int? pageSize = 10)
        {
            if (!pageNumber.HasValue || pageNumber.Value < 1) pageNumber = 1;
            if (!pageSize.HasValue || pageSize.Value < 10) pageSize = 10;

            var userId = _userManager.GetUserId(User);
            UserDashboardModel model = new UserDashboardModel();

            model.Payments7 = 0;
            model.Payments30 = 0;
            model.Orders7 = 0;
            model.Orders30 = 0;

            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
            foreach (var operation in await _context.Operations.Where(operation => operation.OwnerID == userId).ToListAsync())
            {
                if (DateTime.Compare(sevenDaysAgo, operation.Date) >= -1)
                {
                    model.Payments7 += operation.OperationValue;
                    model.Orders7 += _context.BasketItems.Where(item => item.BasketID == operation.BasketID).Sum(item => item.Amount).Value;
                }
                if (DateTime.Compare(thirtyDaysAgo, operation.Date) >= -1)
                {
                    model.Payments30 += operation.OperationValue;
                    model.Orders30 += _context.BasketItems.Where(item => item.BasketID == operation.BasketID).Sum(item => item.Amount).Value;
                }
            }

            var query = _context.Operations.Where(operation => operation.OwnerID == _userManager.GetUserId(User))
                .Include(operation => operation.Location)
                .Include(operation => operation.Location.City)
                .Include(operation => operation.Location.City.Country)
                .Include(operation => operation.Owner);
            model.Operations = await PaginatedList<Operation>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize.Value);
            return View(model);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> AdminBoard(int? pageNumber, int? pageSize = 10)
        {
            AdminDashboardModel model = new AdminDashboardModel();
            model.Earning7 = 0;
            model.Earning30 = 0;
            model.WaitingCustomers = 0;
            model.Orders30 = 0;

            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
            foreach (var operation in await _context.Operations.ToListAsync()) {
                if(DateTime.Compare(sevenDaysAgo, operation.Date) >= -1)
                {
                    model.Earning7 += operation.OperationValue;
                }
                if (DateTime.Compare(thirtyDaysAgo, operation.Date) >= -1)
                {
                    model.Earning30 += operation.OperationValue;
                    model.Orders30 += _context.BasketItems.Where(item => item.BasketID == operation.BasketID).Sum(item => item.Amount).Value;
                }
            }
            model.WaitingCustomers = _context.Operations.Where(operation => operation.Vehicle == null).Count();

            var query = _context.Operations
                .Include(operation => operation.Location)
                .Include(operation => operation.Location.City)
                .Include(operation => operation.Location.City.Country)
                .Include(operation => operation.Owner)
                ;
            model.Operations = await PaginatedList<Operation>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize.Value);

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<ActionResult> AdminDashboardChart(int? lastDays)
        {
            var operations = _context.Operations
                .OrderBy(operation => operation.Date)
                .GroupBy(operation => operation.Date.Date);

            var newestOperations = operations
                .Where(group => group.Key >= DateTime.Now.AddDays(lastDays ?? -30))
                .Select(group => new
                {
                    Date = group.Key.ToShortDateString(),
                    Value = group.Sum(operation => operation.OperationValue)
                });

            var previouslyOperations = operations
                .Where(group => group.Key <= DateTime.Now.AddDays(lastDays ?? -30) && group.Key >= DateTime.Now.AddDays(2 * (lastDays ?? -30)))
                .Select(group => new
                {
                    Date = group.Key.ToShortDateString(),
                    Value = group.Sum(operation => operation.OperationValue)
                });

            var label = await newestOperations.Select(operation => operation.Date).ToListAsync();
            var series = new List<List<DataObject>>() {
                await newestOperations.Select(operation => new DataObject{meta = "This month", value = operation.Value}).ToListAsync(),
                await previouslyOperations.Select(operation => new DataObject{meta = "Previously month", value = operation.Value}).ToListAsync(),
            };


            return Json(new { labels = label, series = series });
        }


        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> CustomerDashboardChart(int? lastDays)
        {
            var operations = _context.Operations
                .Where(operation => operation.OwnerID == _userManager.GetUserId(User))
                .OrderBy(operation => operation.Date)
                .GroupBy(operation => operation.Date.Date);

            var newestOperations = operations
                .Where(group => group.Key >= DateTime.Now.AddDays(lastDays ?? -30))
                .Select(group => new
                {
                    Date = group.Key.ToShortDateString(),
                    Value = group.Sum(operation => operation.OperationValue)
                });

            var previouslyOperations = operations
                .Where(group => group.Key <= DateTime.Now.AddDays(lastDays ?? -30) && group.Key >= DateTime.Now.AddDays(2 * (lastDays ?? -30)))
                .Select(group => new
                {
                    Date = group.Key.ToShortDateString(),
                    Value = group.Sum(operation => operation.OperationValue)
                });

            var label = await newestOperations.Select(operation => operation.Date).ToListAsync();
            var series = new List<List<DataObject>>() {
                await newestOperations.Select(operation => new DataObject{meta = "This month", value = operation.Value}).ToListAsync(),
                await previouslyOperations.Select(operation => new DataObject{meta = "Previously month", value = operation.Value}).ToListAsync(),
            };


            return Json(new { labels = label, series = series });
        }
    }
    public class DataObject
    {
        public string meta { get; set; }
        public double value { get; set; }
    }

    public class UserDashboardModel
    {
        public double Payments7 { get; set; }
        public double Payments30 { get; set; }
        public double Orders7 { get; set; }
        public double Orders30 { get; set; }
        public PaginatedList<Operation> Operations { get; set; }
    }

    public class AdminDashboardModel
    {        
        public double Earning7 { get; set; }
        public double Earning30 { get; set; }
        public double WaitingCustomers { get; set; }
        public double Orders30 { get; set; }
        public PaginatedList<Operation> Operations { get; set; }
    }
}
