using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using EmailService.Model;
using EmailService.Model.Canonical;
using Newtonsoft.Json;
namespace EmailService
{
    internal class Program
    {
       
        static void Main(string[] args)
        {
            Console.Title = "Emailservice";
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Email app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "EmailExchange";
            string queueName = "EmailQueue";
            string routingKey = "EmailRoutingKey";


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
                foreach (var reciever in report.Recievers) 
                {
                    Console.WriteLine($"Sending report to email: {reciever.RecieverEmail} reciever: {reciever.RecieverName}");    
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
