using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Item
    {
        public Item()
        {
            BasketItems = new HashSet<BasketItem>();
        }
        [DisplayName("Item ID")]
        public int ItemID { get; set; }
        [DisplayName("Item Name")]
        public string ItemName { get; set; }
        [DisplayName("Item Price")]
        public double? ItemPrice { get; set; }
        [DisplayName("Item Description")]
        public string ItemDesc { get; set; }
        [DisplayName("Category ID")]
        public int? CategoryID { get; set; }
        public string? LastModifier { get; set; }
        [DisplayName("Category")]
        public virtual Category Category { get; set; }
        public virtual ICollection<BasketItem> BasketItems { get; set; }
    }
}
