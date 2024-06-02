using MauiApp1.View;
namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(ReportDetailView), typeof(ReportDetailView));
            Routing.RegisterRoute(nameof(CreateReportView), typeof(CreateReportView));
        }
    }
}
