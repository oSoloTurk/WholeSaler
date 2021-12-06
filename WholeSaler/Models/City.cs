using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class City
    {
        public City()
        {
            Locations = new HashSet<Location>();
        }
        [DisplayName("City ID")]
        public int CityID { get; set; }
        [DisplayName("City Name")]
        public string CityName { get; set; }
        [DisplayName("Country ID")]
        public int? CountryID { get; set; }
        [DisplayName("Operational State")]
        public bool OperationalState { get; set; }
        [DisplayName("Request Counter")]
        public int? RequestCounter { get; set; }
        [DisplayName("Normalized City Name")]
        public string NormalizedCityName { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
    }
}
