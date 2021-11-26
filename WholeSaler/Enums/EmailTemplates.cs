using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WholeSaler.Enums
{
    public class EmailTemplates 
    { 
        private EmailTemplates(string value) { Value = value; }

        public string Value { get; private set; }

        public static EmailTemplates CONFIRM_ACCOUNT { get { return new EmailTemplates("Confirm_Email_Template.html"); } }
    }
}
