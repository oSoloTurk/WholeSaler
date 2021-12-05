using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Country
    {
        public Country()
        {
            Cities = new HashSet<City>();
        }

        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public bool OperationalState { get; set; }
        public int? RequestCounter { get; set; }
        public string NormalizedCountryName { get; set; }
        public virtual ICollection<City> Cities { get; set; }
    }
}
