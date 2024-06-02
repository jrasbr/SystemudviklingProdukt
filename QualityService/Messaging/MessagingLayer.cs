
using MauiApp1.Model;
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
        //private readonly ConnectivityHelper _connectivityHelper;
        //private readonly FileTransferDatabase _fileTransferDatabase;

        public EventHandler ReportRecievedHandler;
        public MessagingLayer()
        {

        }



        public  async void StartRecieving()
        {

            while (true) 
            {
            
                ConnectionFactory factory = new ConnectionFactory();
                factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
                factory.ClientProvidedName = "Rabbit reciever app 2";

                IConnection cnn = factory.CreateConnection();

                IModel channel = cnn.CreateModel();
                string exchangeName = "QualityExchange";
                string queueName = "QualityQueue";
                string routingKey = "QualityRoutingKey.*";

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                string consumerTag = channel.BasicConsume(queueName, false, consumer);
                consumer.Received += (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Report report = JsonConvert.DeserializeObject<Report>(message);
                    Console.WriteLine("Received {0}", message);
                    ReportRecievedHandler.Invoke(report, null);
                    
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    

                };

                await Task.Delay(1500);
                channel.BasicCancel(consumerTag);
                channel.Close();
                cnn.Close();
            }

        }

        public void SendReport(Report report)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Quality sender app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "QualityExchange";
            string queueName = "QualityQueue";
            string routingKey = $"QualityRoutingKey.{report.PilotId}";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            string jsonMessage = JsonConvert.SerializeObject(report);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            channel.BasicPublish(exchangeName, routingKey, null, messageBytes);

            channel.Close();
            cnn.Close();
        }

        //public async void SendReportImages()
        //{

        //    List<Model.Transfers.FileTransferState> fileTransfers = _fileTransferDatabase.GetAll().Result;

        //    ConnectionFactory factory = new ConnectionFactory();
        //    factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
        //    factory.ClientProvidedName = "Rabbit sender app";

        //    IConnection cnn = factory.CreateConnection();

        //    IModel channel = cnn.CreateModel();
        //    string exchangeName = "DemoExchange";
        //    string queueName = "DemoQueue";
        //    string routingKey = "DemoRoutingKey";

        //    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
        //    channel.QueueDeclare(queueName, true, false, false, null);
        //    channel.QueueBind(queueName, exchangeName, routingKey, null);

        //    foreach (FileTransferState state in fileTransfers)
        //    {
        //        string pathToFile = Path.Combine(FileSystem.CacheDirectory, state.FileId);
                
        //        long startIndex = state.TransferredBytes;
        //        int bufferSize = 1024; // Number of bytes to read at a time

        //        using (var fileStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
        //        {
        //            fileStream.Seek(startIndex, SeekOrigin.Begin);
        //            byte[] buffer = new byte[bufferSize];
        //            int bytesRead;

        //            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //            {
        //                // Process the chunk of data read from the file
        //                byte[] chunk = new byte[bytesRead];
        //                Array.Copy(buffer, chunk, bytesRead);

        //                FileMessage fileMessage = new FileMessage
        //                {
        //                    FileId = state.FileId,
        //                    FileBytes = chunk,
        //                    Length = state.TotalBytes,
        //                    MimeType = state.MimeType,
        //                    Position = startIndex

        //                };
        //                string jsonMessage = JsonConvert.SerializeObject(fileMessage);
        //                byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
        //                // Simulate processing time

        //                // Update the UI with the processed chunk

        //                channel.BasicPublish(exchangeName, routingKey, null, messageBytes);
        //            }
        //        }

        //    }
          

            //channel.BasicPublish(exchangeName, routingKey, null, messageBytes);

        //    channel.Close();
        //    cnn.Close();




        //}

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
