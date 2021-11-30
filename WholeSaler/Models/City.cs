using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class City
    {
        public City()
        {
            Locations = new HashSet<Location>();
        }
        public int CityID { get; set; }
        public string CityName { get; set; }
        public int? CountryID { get; set; }
        public bool OperationalState { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
    }
}
