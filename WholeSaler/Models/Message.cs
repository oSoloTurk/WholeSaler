using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Message
    {
        public int MessageID { get; set; }
        public string MessageText { get; set; }
        public int DateID { get; set; }

        public virtual Date Date { get; set; }
    }
}
