using FileServiceConsole.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FileServiceConsole
{
    internal class Program
    {
        static Dictionary<string, FileTransfer> transfers = new();
        static void Main(string[] args)
        {
            Console.Title = "FileService";
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "FileService app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "FileTransferExchange";
            string queueName = "FileTransferQueue";
            string routingKey = "FileTransferRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                FileMessage fileMessage = JsonConvert.DeserializeObject<FileMessage>(message);
                //transfers.Add(fileMessage.FileId, new FileTransfer { FileId = fileMessage.FileId});
                Console.WriteLine($"Wrote {fileMessage.FileBytes.Length} bytes to {fileMessage.FileId}. {fileMessage.Position + fileMessage.FileBytes.Length}/{fileMessage.Length} recieved");
                Thread.Sleep(500);
                if (fileMessage.Position + fileMessage.FileBytes.Length >= fileMessage.Length)
                {
                    Console.WriteLine($"Filetransfer completed for file with Id: {fileMessage.FileId}");
                    Thread.Sleep(1000);
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            string consumerTag = channel.BasicConsume(queueName, false, consumer);

            Console.ReadLine();

            channel.BasicCancel(consumerTag);
            channel.Close();
            cnn.Close();
        }
    }
}
