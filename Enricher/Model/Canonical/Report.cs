using MauiApp1.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enricher.Model.Canonical
{
    public class Report
    {
        public string ReportId { get; set; }

        public string PilotId { get; set; }
        public string FollowupNote { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; }

        public IList<ReportEvent> Events { get; set; } = new List<ReportEvent>();
        public IList<ReportReciever> Recievers { get; set; } = new List<ReportReciever>();
        public DateTime Date { get; set; } = DateTime.Now;
        public string Location { get; set; }
        public ReportType ReportType { get; set; }
    }
    public class ReportReciever
    {
        public int? RecieverId { get; set; }

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
        public int Id { get; set; }
        public DateTime EventTime { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public ReportEventType ReportType { get; set; }
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
