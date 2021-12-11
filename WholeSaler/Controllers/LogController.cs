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

            IQueryable<Models.Action> wholesellerContext = _context.Actions.Include(action => action.EffecterUserNavigation);
            if (query != null)
            {
                wholesellerContext = wholesellerContext.Where(i => i.ActionDescription.ToString().Contains(query) | i.EffecterUserNavigation.UserName.Contains(query));
                TempData["Query"] = query;
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
                }
                TempData["CurrentFilter"] = sortOrder;

            }
            if (filter != null)
            {
                TempData["Filter"] = filter;
            }

            return View(await PaginatedList<Models.Action>.CreateAsync(wholesellerContext.AsNoTracking(), pageNumber ?? 1, pageSize.Value));
        }

    }
}
