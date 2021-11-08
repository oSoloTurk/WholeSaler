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

namespace WholeSaler.Controllers
{
    public class DashboardController : Controller
    {
        private readonly WholesalerContext _context;

        public DashboardController(WholesalerContext context)
        {
            _context = context;
        }
        public IActionResult User()
        {
            return View();
        }

        public async Task<IActionResult> Admin()
        { 
            int earnWeek = 0;
            int earnMonthly = 0;
            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
            foreach (var operation in await _context.Operations.Include(operation => operation.Date).ToListAsync()) {
                if(DateTime.Compare(sevenDaysAgo, operation.Date.Time) >= -1)
                {
                    earnWeek += operation.OperationValue;
                }
                if (DateTime.Compare(thirtyDaysAgo, operation.Date.Time) >= -1)
                {
                    earnMonthly  += operation.OperationValue;
                }
            }
            ViewData["Earnings7"] = earnWeek;
            ViewData["Earnings30"] = earnMonthly;
            ViewData["WaitingCustomer"] = _context.Operations.Where(operation => operation.Vehicle == null).Count();
            return View();
        }

        public async Task<ActionResult> GetEarnHistory()
        {
            Dictionary<DateTime, double> earns = new Dictionary<DateTime, double>();
            List<Operation> operations = await _context.Operations.Include(operation => operation.Date).ToListAsync();
            DateTime divider = DateTime.Now.AddDays(-1);
            double valueTracker = 0;
            foreach(Operation operation in operations)
            {
                valueTracker += operation.OperationValue;
                if (divider.CompareTo(operation.Date.Time) >= -1)
                {
                    earns.Add(divider, valueTracker);
                    divider = operation.Date.Time.AddDays(-1);
                    continue;

                }
            }
            return Json(earns);
        }
    }
}
