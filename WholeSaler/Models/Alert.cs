using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Alert
    {
        [DisplayName("Alert ID")]
        public int AlertID { get; set; }
        [DisplayName("User ID")]
        public string UserID { get; set; }
        [DisplayName("Message")]
        public string Message { get; set; }
        [DisplayName("Action")]
        public string Action { get; set; }
        [DisplayName("Date ID")]
        public int DateID { get; set; }
        public virtual Date Date { get; set; }
        public virtual User User { get; set; }
    }
}
