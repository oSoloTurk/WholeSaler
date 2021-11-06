using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Category
    {
        public Category()
        {
            Items = new HashSet<Item>();
        }

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}
