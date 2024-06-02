using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Model
{
    public class ReportImage
    {

        [PrimaryKey]
        public string FileId { get; set; }
        
        [ForeignKey(typeof(ReportEvent))]
        public string EventId { get; set; }
        public string MimeType { get; set; }    
        public byte[] Thumb { get; set; }
        public long TotalLength { get; set; }

        public string GetFilePath()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileId);
        }
    }
}
