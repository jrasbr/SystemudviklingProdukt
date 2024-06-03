
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
                string routingKey = $"QualityRoutingKey";//.*

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
            string queueName = $"QualityQueue.{report.PilotId}";
            string routingKey = $"QualityRoutingKey.1";//

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            string jsonMessage = JsonConvert.SerializeObject(report);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            channel.BasicPublish(exchangeName, routingKey, null, messageBytes);

            channel.Close();
            cnn.Close();
        }

    }
}
