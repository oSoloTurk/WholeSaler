using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Country
    {
        public Country()
        {
            Cities = new HashSet<City>();
        }
        [DisplayName("Country ID")]
        public int CountryID { get; set; }
        [DisplayName("Country Name")]
        public string CountryName { get; set; }
        [DisplayName("Operational State")]
        public bool OperationalState { get; set; }
        [DisplayName("Request Counter")]
        public int? RequestCounter { get; set; }
        [DisplayName("Normalized Country Name")]
        public string NormalizedCountryName { get; set; }
        public virtual ICollection<City> Cities { get; set; }
    }
}
