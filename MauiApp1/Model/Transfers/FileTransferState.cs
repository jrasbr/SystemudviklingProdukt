using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Model.Transfers
{
    public class FileTransferState
    {
        [PrimaryKey]
        public string FileId { get; set; }
        public string MimeType { get; set; }
        public long TotalBytes{ get; set; }
        public long TransferredBytes { get; set; }
    }
}
