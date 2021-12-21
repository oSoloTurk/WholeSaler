using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="SuperAdmin, Admin, Customer")]
    public class AlertController : ControllerBase
    {
        private readonly WholesalerContext _context;
        private readonly UserManager<User> _userManager;

        public AlertController(WholesalerContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Alert
        [HttpGet]
        public async Task<Alerts> Get()
        {
            var userId = _userManager.GetUserId(User);
            var alerts = new Alerts
            {
                Elements = await _context.Alerts.Where(alert => alert.UserID == userId).Select(alert => new AlertView()
                {
                    Date = alert.Date.ToLongDateString() + " | " + alert.Date.ToLongTimeString(),
                    Message = alert.Message,
                    Redirect =  alert.Redirect,
                }).ToListAsync()
            };
            alerts.AlertCount = alerts.Elements.Count;
            return alerts;
        }

        // DELETE: api/Alert
        [HttpDelete]
        public async Task Delete()
        {
            var userId = _userManager.GetUserId(User);
            await _context.Alerts.Where(alert => alert.UserID == userId).ForEachAsync(alert => _context.Alerts.Remove(alert));
            await _context.SaveChangesAsync();
        }
    }

    public class Alerts
    {
        public int AlertCount { get; set; }
        public List<AlertView> Elements { get; set;}
    }

    public class AlertView
    {
        public string Message { get; set; }
        public string Redirect { get; set; }
        public string Date { get; set; }
    }
}
