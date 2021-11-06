using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace WholeSaler.Models
{
    [Table("AspNetUsers")]
    public class User : IdentityUser
    {
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
    }
}
