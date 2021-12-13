using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Utils;

namespace WholeSaler.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class LogController : Controller
    {
        private readonly WholesalerContext _context;

        public LogController(WholesalerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string filter,string sortOrder, string query, int? pageNumber, int? pageSize = 5)
        {
            if (!pageNumber.HasValue || pageNumber.Value < 1) pageNumber = 1;
            if (!pageSize.HasValue || pageSize.Value < 10) pageSize = 10;

            IQueryable<Models.Action> wholesellerContext = _context.Actions;
            if (query != null)
            {
                wholesellerContext = wholesellerContext.Where(i => i.ActionDescription.ToString().Contains(query) | i.EffecterUser.Contains(query));
                TempData["Query"] = query;
            }
            if (filter != null)
            {
                switch (filter)
                {
                    default:
                        break;
                    case "product_changes":
                        wholesellerContext = wholesellerContext.Where(item => item.ActionElement.Equals("Items"));
                        break;
                    case "adress_changes":
                        wholesellerContext = wholesellerContext.Where(item => item.ActionElement.Equals("Locations"));
                        break;
                    case "vehicle_changes":
                        wholesellerContext = wholesellerContext.Where(item => item.ActionElement.Equals("Vehicles"));
                        break;
                    case "operation_changes":
                        wholesellerContext = wholesellerContext.Where(item => item.ActionElement.Equals("Operations"));
                        break;
                }
                TempData["Filter"] = filter;
            }

            if (sortOrder != null)
            {
                switch (sortOrder)
                {
                    default:
                    case "log_date":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.Date);
                        break;
                    case "log_effecter":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.EffecterUser);
                        break;
                    case "log_desc":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.ActionDescription);
                        break;
                    case "log_element":
                        wholesellerContext = wholesellerContext.OrderBy(item => item.ActionElement);
                        break;
                }
                TempData["CurrentFilter"] = sortOrder;

            }
            return View(await PaginatedList<Models.Action>.CreateAsync(wholesellerContext.AsNoTracking(), pageNumber ?? 1, pageSize.Value));
        }

    }
}
