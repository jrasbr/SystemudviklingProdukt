using MauiApp1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityService.ViewModel
{
    public class ReportVM
    {
        private bool _followUp;

        public ReportVM()
        {
            Model = new Report();
        }
        public ObservableCollection<ReportEvent> Events 
        {
            get { return new ObservableCollection<ReportEvent>(Model.Events); } 
        }

        public Report Model { get; set; }

        public bool FollowUp 
        {
            get => _followUp;
            set
            { 
                _followUp = value;
                Model.ReportType = value ? ReportType.FollowUp : ReportType.Normal;
            } 
        }

    }
    
}
