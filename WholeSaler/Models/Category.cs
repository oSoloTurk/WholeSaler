using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Category
    {
        public Category()
        {
            Items = new HashSet<Item>();
        }
        [DisplayName("Category ID")]
        public int CategoryID { get; set; }
        [DisplayName("Category Name")]
        public string CategoryName { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
