using MauiApp1.Messaging;
using MauiApp1.Model;
using QualityService.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QualityService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private MessagingLayer _messagingLayer;

        public ObservableCollection<ReportVM> Reports { get; set; } = new ObservableCollection<ReportVM>();
        
        //Bind to enum values
        public List<string> ReportTypes
        {
            get { return Enum.GetNames(typeof(ReportType)).ToList(); }
        }
        

        private ReportVM _selectedReport;
        public ReportVM SelectedReport
        {
            get => _selectedReport;
            set
            {
                _selectedReport = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _messagingLayer = new MessagingLayer();
            _messagingLayer.ReportRecievedHandler += (s, e) =>
            {
                if (s is Report report)
                {
                    bool found = false;
                    for (int reportIdIndex = 0; reportIdIndex < Reports.Count; reportIdIndex++)
                    {
                        ReportVM existingReport = Reports[reportIdIndex];
                        if (report.ReportId == existingReport.Model.ReportId)
                        {

                            Dispatcher.BeginInvoke(new Action(() =>
                            {  
                                Reports.Add(new ReportVM { Model = report });
                                Reports.Remove(existingReport);
                            }));

                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Reports.Add(new ReportVM { Model = report });
                        }));

                    }
                }
            };

            _messagingLayer.StartRecieving();

            Load();

        }


        private void Load()
        {
            Reports.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            object selectedItem = listView.SelectedItem;
            if (selectedItem is ReportVM vm)
            {
                SelectedReport = vm;

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedReport == null)
            {
                return;
            }

          _messagingLayer.SendReport(SelectedReport.Model);
            SelectedReport = null;
        }
    }
}