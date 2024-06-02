using MauiApp1.Messaging;
using MauiApp1.Model;
using MauiApp1.Repository;
using MauiApp1.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.ViewModel
{
    public class ReportsViewModel
    {
        public ObservableCollection<Report> Model { get; set; } = new ObservableCollection<Report>();


        private Report _selectedReport;
        public Report SelectedReport
        { 
            get => _selectedReport;
            set
            {
                _selectedReport = value;
                if (_selectedReport == null)
                {
                    return;
                }
                //Navigate to the detail view
                OpenDetailView(_selectedReport);
            }
        }

        private async void OpenDetailView(Report selectedReport)
        {
            await Shell.Current.GoToAsync($"{nameof(ReportDetailView)}?reportId={selectedReport.ReportId}");
        }

        private ReportDatabase _db;
        private readonly MessagingLayer _messagingLayer;

        public ReportsViewModel(ReportDatabase db, MessagingLayer messagingLayer)
        {
            _db = db;
            _messagingLayer = messagingLayer;
            _messagingLayer.ReportChangesHandler += (s, e) => LoadData();

            NewReportCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(CreateReportView));
            });
        }

        public async void LoadData()
        {
            Model.Clear();
            var reports = await _db.GetAll();
            foreach (var report in reports)
            {
                Model.Add(report);
            }
        }

        public Command NewReportCommand { get; set; }
    }
}
