using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Data;
using WholeSaler.Models;

namespace WholeSaler.Utils
{
    public class AlertService
    {
        private WholesalerContext _context;

        public AlertService(WholesalerContext context)
        {
            _context = context;
        }

        public async void SendAlert(User targetUser, string message, string action)         
        {
            Alert alert = new()
            {
                Redirect = action,
                User = targetUser,
                UserID = targetUser.Id,
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
