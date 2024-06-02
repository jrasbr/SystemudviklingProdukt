using MauiApp1.Util;
using SQLite;
using SQLiteNetExtensions.Attributes;
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
        public int CustomerId { get; set; }

        public string PilotId { get; set; } = Constants.USER_ID;
        public string FollowupNote { get; set; }
        public string Title { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public IList<ReportEvent> Events { get; set; } = new List<ReportEvent>();

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public IList<ReportReciever> Recievers { get; set; } = new List<ReportReciever>() { new ReportReciever() { RecieverName = "Quality" } };
       
        public DateTime Date { get; set; } = DateTime.Now;
        public string Location { get; set; }
        public ReportType ReportType { get; set; }
    }
    public class ReportReciever
    {
        [PrimaryKey]
        [AutoIncrement]
        public string RecieverId { get; set; }

        [ForeignKey(typeof(Report))]
        public string ReportId { get; set; }
        public string RecieverName { get; set; }
        public string RecieverEmail { get; set; }
    }
    public enum ReportType
    {
        Normal = 0,
        FollowUp = 1,
    }

    public class ReportEvent 
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Report))]
        public string ReportId { get; set; }
        public DateTime EventTime { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string Note { get; set; }
        public ReportEventType ReportType { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<ReportImage> Images { get; set; } = new List<ReportImage>();
    }
     
    public enum ReportEventType
    {
        Normal = 0,
        Accident = 1,
        Delay = 2,
        Other = 3
    }
}
