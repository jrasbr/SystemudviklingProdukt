using MauiApp1.Connectivity;
using MauiApp1.Database;
using MauiApp1.Messaging;
using MauiApp1.Repository;
using MauiApp1.View;
using MauiApp1.ViewModel;
using Microsoft.Extensions.Logging;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .Services.AddSingleton<ReportDatabase>()
                .AddSingleton<FileTransferDatabase>()
                .AddSingleton<ConnectivityHelper>()
                .AddSingleton<MessagingLayer>()
                .AddSingleton<ReportsView>()
                .AddTransient<CreateReportView>()
                .AddTransient<ReportDetailView>()
                .AddSingleton<ReportsViewModel>()
                .AddTransient<ReportDetailViewModel>()
                .AddTransient<CreateReportViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
