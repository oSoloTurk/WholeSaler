using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Location
    {
        public Location()
        {
            Operations = new HashSet<Operation>();
        }

        public int LocationID { get; set; }
        public string LocationOwnerID { get; set; }
        public string Adress { get; set; }
        public int? CityID { get; set; }
        public virtual User LocationOwner { get; set; }
        public virtual City City { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }

        public String GetFullAdress()
        {
            return Adress + "\n" + City.CityName + "-" + City.Country.CountryName;
        }
    }
}
