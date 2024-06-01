using MauiApp1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.ViewModel
{
    [QueryProperty("ReportId", "reportId")]
    public class ReportDetailViewModel : BindableObject
    {
        private
        public Report Model { get; set; }
        
        private string reportId;
        public string ReportId 
        {
            get => reportId;
            set { reportId = value; }
        }

        public ReportDetailViewModel(string reportId)
        {
            
        }
    }
}
