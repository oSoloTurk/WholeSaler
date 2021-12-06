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
    public class DashboardController : Controller
    {
        private readonly WholesalerContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(WholesalerContext context, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
        }
        [Authorize(Roles = "Customer")]

        public async Task<IActionResult> UserBoard(int? pageNumber, int? pageSize = 5)
        {
            if (!pageNumber.HasValue || pageNumber.Value < 1) pageNumber = 1;
            if (!pageSize.HasValue || pageSize.Value < 10) pageSize = 10;

            UserDashboardModel model = new UserDashboardModel();
            var query = _context.Operations.Where(operation => operation.OwnerID == _userManager.GetUserId(User));
            model.Operations = await PaginatedList<Operation>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize.Value);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminBoard(int? pageNumber, int? pageSize = 5)
        { 
            int earnWeek = 0;
            int earnMonthly = 0;
            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
            foreach (var operation in await _context.Operations.ToListAsync()) {
                if(DateTime.Compare(sevenDaysAgo, operation.Date) >= -1)
                {
                    earnWeek += operation.OperationValue;
                }
                if (DateTime.Compare(thirtyDaysAgo, operation.Date) >= -1)
                {
                    earnMonthly  += operation.OperationValue;
                }
            }
            ViewData["Earnings7"] = earnWeek;
            ViewData["Earnings30"] = earnMonthly;
            ViewData["WaitingCustomer"] = _context.Operations.Where(operation => operation.Vehicle == null).Count();

            AdminDashboardModel model = new AdminDashboardModel();
            var query = _context.Operations;
            model.Operations = await PaginatedList<Operation>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize.Value);

            return View(model);
        }
    }
    public class UserDashboardModel
    {
        public PaginatedList<Operation> Operations { get; set; }
    }

    public class AdminDashboardModel
    {
        public PaginatedList<Operation> Operations { get; set; }
    }
}
