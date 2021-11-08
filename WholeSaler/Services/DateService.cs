using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WholeSaler.Data;
using WholeSaler.Models;
using Microsoft.AspNetCore.Http;
using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace WholeSaler.Utils
{
    public static class DateService
    {
        //check initalize method is correct
        private static WholesalerContext _context = new WholesalerContext();

        // GET: Create Date and Return ID
        public static Date CreateDate()
        {
            Date date = new Date();
            //initalize date
            return null;
        }

        public static async void DisposeDate(int dateId)
        {
            Date date = await _context.Dates.FindAsync(dateId);
            _context.Dates.Remove(date);
        }
    }
}
