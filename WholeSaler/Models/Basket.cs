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
            BasketItems = new HashSet<BasketItem>();
        }
        [Key]
        public int BasketID { get; set; }
        public DateTime Date { get; set; }
        public string UserID { get; set; }
        public bool IsArchived { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<BasketItem> BasketItems { get; set; }
        public virtual ICollection<Operation> Operations { get; set; }
    }
}
