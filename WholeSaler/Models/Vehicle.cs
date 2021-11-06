using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Vehicle
    {
        public Vehicle()
        {
            Operations = new HashSet<Operation>();
        }

        public int VehicleID { get; set; }
        public string VehicleName { get; set; }
        public string VehiclePlate { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }
    }
}
