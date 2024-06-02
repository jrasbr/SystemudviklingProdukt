using MauiApp1.Database;
using MauiApp1.Messaging;
using MauiApp1.Model;
using MauiApp1.Model.Transfers;
using MauiApp1.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SQLite.SQLite3;

namespace MauiApp1.ViewModel
{
    public class CreateReportViewModel : BindableObject
    {
        private readonly ReportDatabase _reportDatabase;
        private readonly MessagingLayer _messagingLayer;
        private readonly FileTransferDatabase _fileTransferDatabase;

        public Report Model { get; set; } = new Report();
        public ObservableCollection<ReportEvent> Events { get; set; } = new ObservableCollection<ReportEvent>();

        private ReportEvent _reportEvent = new ReportEvent();
        private int _selectedEventTypeIndex;

        //I need to bind picler to the ReportType property
        public int SelectedEventTypeIndex
        {
            get => _selectedEventTypeIndex;
            set { _selectedEventTypeIndex = value; OnPropertyChanged(); 
                ReportEvent.ReportType = (ReportEventType)value;
            }
        }

        public List<string> ReportTypes
        {
            get { return Enum.GetNames(typeof(ReportEventType)).ToList(); }
        }

        public ReportEvent ReportEvent
        {
            get => _reportEvent;
            set { _reportEvent = value; OnPropertyChanged(); }
        }
        public CreateReportViewModel(ReportDatabase reportDatabase, MessagingLayer messagingLayer, FileTransferDatabase fileTransferDatabase)
        {
            _reportDatabase = reportDatabase;
            _messagingLayer = messagingLayer;
            _fileTransferDatabase = fileTransferDatabase;
            AddEventCommand = new Command(async () =>
            {
                var reportEvent = new ReportEvent
                {
                    ReportId = Model.ReportId,
                    EventTime = ReportEvent.EventTime,
                    Description = ReportEvent.Description,
                    Note = ReportEvent.Note,
                    ReportType = ReportEvent.ReportType,
                    Images = ReportEvent.Images
                };
                Model.Events.Add(reportEvent);
                Events.Add(reportEvent);
                await _reportDatabase.Save(Model);
                ReportEvent = new ReportEvent();
                //Model = new Report();
                SelectedEventTypeIndex = 0;
            });

            AddImageCommand = new Command(async () =>
            {

                ReportImage res = await TakePhoto();
                if (res != null)
                {
                    ReportEvent.Images.Add(res);
                }


            });
            SaveReportCommand = new Command(ExecuteSaveReport);


        }

        private async void ExecuteSaveReport(object obj)
        {
            try
            {
                foreach (var reportEvent in Model.Events)
                {
                    foreach (ReportImage image in reportEvent.Images)
                    {
                        await _fileTransferDatabase.Save(new FileTransferState { FileId = image.FileId, MimeType = image.MimeType, TotalBytes = image.TotalLength, TransferredBytes = 0 });
                    }
                }
                await _reportDatabase.Save(Model);
                _messagingLayer.SendReport(Model);

                await Shell.Current.GoToAsync($"..");

            }
            catch (Exception e)
            {

                throw;
            }

        }

        public async Task<ReportImage> TakePhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {

                FileResult photo = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images
                });

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    var fileName = photo.FileName;
                    var fullPath = photo.FullPath;

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);

                    await sourceStream.CopyToAsync(localFileStream);

                    long length = new FileInfo(localFilePath).Length;

                    ReportImage reportImage = new ReportImage
                    {
                        FileId = photo.FileName,
                        MimeType = "image/jpeg",
                        TotalLength = length,
                        //Thumb = await File.ReadAllBytesAsync(localFilePath)
                    };
                    return reportImage;
                }
            }
            return default;
        }

        public Command AddImageCommand { get; set; }
        public async Task SaveReport()
        {
            await _reportDatabase.Save(Model);
        }

        public Command AddEventCommand { get; set; }
        public Command SaveReportCommand { get; set; }



    }
}
