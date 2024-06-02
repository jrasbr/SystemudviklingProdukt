using MauiApp1.Connectivity;
using MauiApp1.Database;
using MauiApp1.Model;
using MauiApp1.Model.Dto;
using MauiApp1.Model.Transfers;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Messaging
{
    public class MessagingLayer
    {
        private readonly ConnectivityHelper _connectivityHelper;
        private readonly FileTransferDatabase _fileTransferDatabase;
        public EventHandler<Report> ReportChangesHandler;

        public MessagingLayer(ConnectivityHelper connectivityHelper, FileTransferDatabase fileTransferDatabase)
        {
            _connectivityHelper = connectivityHelper;
            _fileTransferDatabase = fileTransferDatabase;
            _connectivityHelper.ConnectionChangedHandler += ConnectivityHelper_ConnectionChangedHandler;
            Task.Run(RecieveReports);
        }

        private void ConnectivityHelper_ConnectionChangedHandler(object? sender, NetworkStateArgs e)
        {
            if (e.IsConnected)
            {
                SendReportImages();
                //SendReport(new Report());
                RecieveReports();
            }
        }

        private async void RecieveReports()
        {
            try
            {


                Thread thread = new Thread(async () =>
                {
                    while (true)
                    {

                        ConnectionFactory factory = new ConnectionFactory();
                        factory.Uri = new System.Uri("amqp://guest:guest@10.0.2.2:5672");
                        factory.ClientProvidedName = "Rabbit reciever app";

                        IConnection cnn = factory.CreateConnection();

                        IModel channel = cnn.CreateModel();


                        string exchangeName = "QualityExchange";
                        string queueName = "QualityQueue";
                        string routingKey = $"QualityRoutingKey.{MauiApp1.Util.Constants.USER_ID}";

                        channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
                        channel.QueueDeclare(queueName, true, false, false, null);
                        channel.QueueBind(queueName, exchangeName, routingKey, null);

                        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            byte[] body = ea.Body.ToArray();
                            string message = Encoding.UTF8.GetString(body);
                            Report report = JsonConvert.DeserializeObject<Report>(message);
                            Console.WriteLine("Received {0}", message);
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            ReportChangesHandler?.Invoke(report, null);
                        };

                        string consumerTag = channel.BasicConsume(queueName, false, consumer);


                        if (!_connectivityHelper.IsConnected)
                        {
                            break;
                        }
                        channel.BasicCancel(consumerTag);
                        channel.Close();
                        cnn.Close();
                        await Task.Delay(200);
                    }
                });

                thread.Start();

            }
            catch (Exception e)
            {

            }
        }

        public void SendReport(Report report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            try
            {


                ConnectionFactory factory = new ConnectionFactory();
                factory.Uri = new System.Uri("amqp://guest:guest@10.0.2.2:5672");
                factory.ClientProvidedName = "Sender client ";

                IConnection cnn = factory.CreateConnection();

                IModel channel = cnn.CreateModel();
                //string exchangeName = "DemoExchange";
                //string queueName = "DemoQueue";
                //string routingKey = "DemoRoutingKey"; 
                string exchangeName = "RecieverExchange";
                string queueName = "RecieverQueue";
                string routingKey = "RecieverRoutingKey";

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);

                string jsonMessage = JsonConvert.SerializeObject(report);
                byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

                channel.BasicPublish(exchangeName, routingKey, null, messageBytes);

                channel.Close();
                cnn.Close();

                //foreach (var file in report.Events) 
                //{
                //    foreach (var image in file.Images)
                //    {
                //        FileTransferState fileTransferState = new FileTransferState
                //        {
                //            FileId = image.FileId,
                //            MimeType = image.MimeType,
                //            TotalBytes = image.Length,
                //            TransferredBytes = 0
                //        };
                //        _fileTransferDatabase.Save(fileTransferState);
                //    }
                //}
                SendReportImages();
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async void SendReportImages()
        {
            try
            {


                List<Model.Transfers.FileTransferState> fileTransfers = await _fileTransferDatabase.GetAll();

                ConnectionFactory factory = new ConnectionFactory();
                factory.Uri = new System.Uri("amqp://guest:guest@10.0.2.2:5672");
                factory.ClientProvidedName = "Client sender app media";

                IConnection cnn = factory.CreateConnection();

                IModel channel = cnn.CreateModel();
                string exchangeName = "FileTransferExchange";
                string queueName = "FileTransferQueue";
                string routingKey = "FileTransferRoutingKey";

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);

                foreach (FileTransferState state in fileTransfers)
                {
                    string pathToFile = Path.Combine(FileSystem.CacheDirectory, state.FileId);

                    long startIndex = state.TransferredBytes;
                    int bufferSize = 12976; // Number of bytes to read at a time

                    using (var fileStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Seek(startIndex, SeekOrigin.Begin);
                        byte[] buffer = new byte[bufferSize];
                        int bytesRead;

                        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            // Process the chunk of data read from the file
                            byte[] chunk = new byte[bytesRead];
                            Array.Copy(buffer, chunk, bytesRead);

                            FileMessage fileMessage = new FileMessage
                            {
                                FileId = state.FileId,
                                FileBytes = chunk,
                                Length = state.TotalBytes,
                                MimeType = state.MimeType,
                                Position = startIndex

                            };
                            string jsonMessage = JsonConvert.SerializeObject(fileMessage);
                            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
                            startIndex += chunk.Length;
                            await Task.Delay(1000);
                            // Simulate processing time

                            // Update the UI with the processed chunk

                            channel.BasicPublish(exchangeName, routingKey, null, messageBytes);
                        }

                    }
                    await _fileTransferDatabase.Delete(state.FileId);

                }


                //channel.BasicPublish(exchangeName, routingKey, null, messageBytes);

                channel.Close();
                cnn.Close();

            }
            catch (Exception)
            {

            }


        }

        //  private async Task ReadFileInChunksAsync(string filePath, int bufferSize, long startIndex)
        //{
        //    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //    {
        //        fileStream.Seek(startIndex, SeekOrigin.Begin);
        //        byte[] buffer = new byte[bufferSize];
        //        int bytesRead;

        //        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //        {
        //            // Process the chunk of data read from the file
        //            string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //            // Simulate processing time
        //            await Task.Delay(100);
        //            // Update the UI with the processed chunk
        //            Dispatcher.Dispatch(() => {
        //                OutputLabel.Text += chunk;
        //            });
        //        }
        //    }
        //}
        //}
    }
}
