using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enricher.Model.Data
{
    public class CustomerContact
    {
        public int CustomerId { get; set; }
        public int CustomerContactId { get; set; }
        public string ContactName { get; set; }
        public string RecieverEmail { get; set; }
        public string? ContactType { get; internal set; }
    }
}
