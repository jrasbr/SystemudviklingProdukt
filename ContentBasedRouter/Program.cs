using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ContentBasedRouter.Model.Canonical;
using ContentBasedRouter.Model;
namespace ContentBasedRouter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "ContentBased Router";
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "ContentBasedRouter app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "ContentRouterExchange";
            string queueName = "ContentRouterQueue";
            string routingKey = "ContentRouterRoutingKey";


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
                
                bool informInsurance = false;
                foreach (var portEvent in report.Events)
                {   switch (portEvent.ReportType)
                    {
                        case ReportEventType.Normal:
                            break;
                        case ReportEventType.Accident:
                            informInsurance = true;
                            break;
                        default:
                            break;
                    }
                }

                if (informInsurance)
                {
                    MessageEmailService(report);
                }

                MessageQualityService(report);


                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            string consumerTag = channel.BasicConsume(queueName, false, consumer);

            Console.ReadLine();

            channel.BasicCancel(consumerTag);
            channel.Close();
            cnn.Close();
        }

        public static void MessageQualityService(Report report) 
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Rabbit sender app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "QualityExchange";
            string queueName = "QualityQueue";
            string routingKey = "QualityRoutingKey.Routed";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            string json = JsonConvert.SerializeObject(report);
            byte[] messageBytes = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchangeName, routingKey, null, messageBytes);
            Console.WriteLine("Sent report to quality service");
            channel.Close();
            cnn.Close();
        }

        public static void MessageEmailService(Report report) 
        {

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Rabbit sender app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "EmailExchange";
            string queueName = "EmailQueue";
            string routingKey = "EmailRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            string json = JsonConvert.SerializeObject(report);
            byte[] messageBytes = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchangeName, routingKey, null, messageBytes);
            Console.WriteLine("Sent report to email service");
            channel.Close();
            cnn.Close();
        }
    }
}
