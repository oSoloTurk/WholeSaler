using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Operation
    {
        [DisplayName("Operation ID")]
        public int OperationID { get; set; }
        [DisplayName("Operation Value")]
        public double OperationValue { get; set; }
        [DisplayName("Date")] 
        public DateTime Date { get; set; }
        [DisplayName("Basket ID")]
        public int? BasketID { get; set; }
        [DisplayName("Location ID")]
        public int? LocationID { get; set; }
        [DisplayName("Vehicle ID")]
        public int? VehicleID { get; set; }
        [DisplayName("Owner ID")]
        public string OwnerID { get; set; }
        public string? LastModifier { get; set; }
        public virtual User Owner { get; set; }
        public virtual Basket Basket { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual Location Location { get; set; }
    }
}
