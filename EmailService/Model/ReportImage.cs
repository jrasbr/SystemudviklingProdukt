
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Model
{
    public class ReportImage
    {

        public string FileId { get; set; }
        
        public string EventId { get; set; }
        public string MimeType { get; set; }    
        public byte[] Thumb { get; set; }
    }
}
