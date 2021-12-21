using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Controllers;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Components
{
    public class AlertsViewComponent : ViewComponent
    {
        private readonly WholesalerContext _context;
        private readonly UserManager<User> _userManager;

        public AlertsViewComponent(WholesalerContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(Request.HttpContext.User);
            var alerts = new Alerts
            {
                Elements = await _context.Alerts.Where(alert => alert.UserID == userId).Select(alert => new AlertView()
                {
                    Date = alert.Date.ToLongDateString() + " | " + alert.Date.ToLongTimeString(),
                    Message = alert.Message,
                    Redirect = alert.Redirect,
                }).ToListAsync()
            };
            alerts.AlertCount = alerts.Elements.Count;
            return View(alerts);
        }
    }
}
