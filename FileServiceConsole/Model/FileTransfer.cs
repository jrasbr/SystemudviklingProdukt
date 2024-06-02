using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileServiceConsole.Model
{
    public class FileTransfer
    {
        public string FileId { get; set; }
        public string MimeType { get; set; }
        public long Position { get; set; }//Current
        public byte[] FileBytes { get; set; }

    }
}
