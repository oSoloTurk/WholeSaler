using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    [Table("AspNetUsers")]
    public class User : IdentityUser
    {
        [DisplayName("Name")]
        public string Name { get; set; }
        [DisplayName("Surname")]
        public string SurName { get; set; }
        [DisplayName("Company Name")]
        public string CompanyName { get; set; }
        [NotMapped]
        public virtual ICollection<Basket> Baskets { get; set; }
        [NotMapped]
        public virtual ICollection<Location> Locations { get; set; }
        [NotMapped]
        public virtual ICollection<Action> ActionAffectedUserNavigations { get; set; }
        [NotMapped]
        public virtual ICollection<Action> ActionEffecterUserNavigations { get; set; }
        [NotMapped]
        public virtual ICollection<Operation> OperationNavigations { get; set; }
        public virtual ICollection<Alert> Alerts { get; set; }
    }
}
