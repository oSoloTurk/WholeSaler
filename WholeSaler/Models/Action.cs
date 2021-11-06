using System;
using System.Collections.Generic;

#nullable disable

namespace WholeSaler.Models
{
    public partial class Action
    {
        public int ActionID { get; set; }
        public string EffecterUser { get; set; }
        public string AffectedUser { get; set; }
        public int DateID { get; set; }
        public virtual User AffectedUserNavigation { get; set; }
        public virtual Date Date { get; set; }
        public virtual User EffecterUserNavigation { get; set; }
    }
}
