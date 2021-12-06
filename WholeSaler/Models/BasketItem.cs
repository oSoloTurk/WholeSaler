using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace WholeSaler.Models
{
    public partial class BasketItem
    {
        [Key]
        public int BasketItemID { get; set; }
        public int BasketID { get; set; }
        public double BasketPrice { get; set; }
        public int ItemID { get; set; }
        public int? Amount { get; set; }
        public virtual Basket Basket { get; set; }
        public virtual Item Item { get; set; }
    }
}
