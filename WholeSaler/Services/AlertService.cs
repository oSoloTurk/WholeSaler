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

        public void SendAlert(User targetUser, string message, string action)         
        {
                Alert alert = new Alert();
                alert.Action = action;
                alert.User = targetUser;
                alert.UserID = targetUser.Id;
                alert.Message = message;
                alert.Date = DateTime.Now;
                _context.Add(new Alert());
        }

        public async void DisposeAlert(int alertId)
        {
            Alert alert = await _context.Alerts.FindAsync(alertId);
            _context.Alerts.Remove(alert);
        }
    }
}
