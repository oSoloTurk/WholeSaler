using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Location
    {
        public Location()
        {
            Operations = new HashSet<Operation>();
        }
        [DisplayName("Location ID")]
        public int LocationID { get; set; }
        [DisplayName("Location Owner ID")]
        public string LocationOwnerID { get; set; }
        [DisplayName("Adress")]
        public string Adress { get; set; }
        [DisplayName("City ID")]
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
