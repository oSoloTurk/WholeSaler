using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Vehicle
    {
        public Vehicle()
        {
            Operations = new HashSet<Operation>();
        }

        [DisplayName("Vehicle ID")]
        public int VehicleID { get; set; }

        [DisplayName("Vehicle Name")]
        public string VehicleName { get; set; }

        [DisplayName("Vehicle Plate")]
        public string VehiclePlate { get; set; }
        public string? LastModifier { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }
    }
}
