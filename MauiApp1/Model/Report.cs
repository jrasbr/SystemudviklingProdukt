using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Model
{
    public class Report
    {
        [PrimaryKey]
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public List<ReportEvent> Events { get; set; }
        public List<string> ImageIds { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
    }

    public class ReportEvent 
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public string ReportId { get; set; }
        public DateTime EventTime { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public ReportEventType ReportType { get; set; }
    }
     
    public enum ReportEventType
    {
        Normal = 0,
        Accident = 1,
        Delay = 2,
        Other = 3
    }
}
