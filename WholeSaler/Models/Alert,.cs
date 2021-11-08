using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Alert
    {
        public int AlertID { get; set; }
        public string UserID { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }
        public int DateID { get; set; }
        public virtual Date Date { get; set; }
        public virtual User User { get; set; }
    }
}
