using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Operation
    {
        public int OperationID { get; set; }
        public int OperationValue { get; set; }
        public int? DateID { get; set; }
        public int? BasketID { get; set; }
        public int? LocationID { get; set; }
        public int? VehicleID { get; set; }
        public string OwnerID { get; set; }
        public virtual User Owner { get; set; }
        public virtual Basket Basket { get; set; }
        public virtual Date Date { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual Location Location { get; set; }
    }
}
