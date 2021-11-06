using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Basket
    {
        public Basket()
        {
            Operations = new HashSet<Operation>();
        }
        [Key]
        public int BasketID { get; set; }
        public int DateID { get; set; }
        public string UserID { get; set; }

        public virtual Date Date { get; set; }
        public virtual User User { get; set; }
        public virtual BasketItem BasketItem { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }
    }
}
