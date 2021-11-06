using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Date
    {
        public Date()
        {
            Actions = new HashSet<Action>();
            Baskets = new HashSet<Basket>();
            Messages = new HashSet<Message>();
            OperationDetails = new HashSet<Operation>();
        }

        public int DateID { get; set; }
        public string UserIp { get; set; }
        public string UserPlatform { get; set; }
        public DateTime Time { get; set; }

        public virtual ICollection<Action> Actions { get; set; }
        public virtual ICollection<Basket> Baskets { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Operation> OperationDetails { get; set; }
    }
}
