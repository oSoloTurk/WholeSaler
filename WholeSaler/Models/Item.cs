using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Item
    {
        public Item()
        {
            BasketItems = new HashSet<BasketItem>();
        }

        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int? ItemPrice { get; set; }
        public string ItemDesc { get; set; }
        public int? CategoryID { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<BasketItem> BasketItems { get; set; }
    }
}
