using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Services
{
    public class AlertService
    {
        private WholesalerContext _context;

        public AlertService(WholesalerContext context)
        {
            _context = context;
        }

        public async Task SendAlert(string targetUser, string message, string action)         
        {
            Alert alert = new()
            {
                Redirect = action,
                UserID = targetUser,
                Message = message,
                Date = DateTime.Now
            };
            _context.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async void DisposeAlert(int alertId)
        {
            Alert alert = await _context.Alerts.FindAsync(alertId);
            _context.Alerts.Remove(alert);
        }
    }
}
