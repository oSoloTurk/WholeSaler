using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Action
    {
        [DisplayName("Action ID")]
        public int ActionID { get; set; }
        [DisplayName("Efect User")]
        public string EffecterUser { get; set; }
        [DisplayName("Action")]
        public string ActionDescription { get; set; }
        [DisplayName("Date")]
        public DateTime Date { get; set; }
        public virtual User EffecterUserNavigation { get; set; }
    }
}
