using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enso.api.Models
{
    public class Email
    {
        public string To { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
