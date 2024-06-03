using Enricher.Database;
using Enricher.Model.Canonical;
using Enricher.Model.Data;
using MauiApp1.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Enricher
{
    internal class Program
    {
        private static  RecieverDatabase _recieverDatabase;
        static void Main(string[] args)
        {
            Console.Title = "Enricher";
            _recieverDatabase = new RecieverDatabase();

            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Enricher app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "RecieverExchange";
            string queueName = "RecieverQueue";
            string routingKey = "RecieverRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Report report = null;
                try
                {
                 report = JsonConvert.DeserializeObject<Report>(message);
                    if (report == null)
                    {
                        Console.WriteLine("Error - redirectint to dead letter queue");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        return;
                    }
                   
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error - redirectint to dead letter queue");
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    return;
                }
                if (report.Events.Count > 0)
                {
                    bool contactInsurance = false;
                    foreach (var reportEvent in report.Events)
                    {
                        switch (reportEvent.ReportType)
                        {
                            case ReportEventType.Normal:
                                break;
                            case ReportEventType.Accident:
                            case ReportEventType.Delay:
                                contactInsurance = true;
                                break;
                            case ReportEventType.Other:
                                break;
                            default:
                                break;
                        }
                    }
                    Customer customer = _recieverDatabase.GetCustomer(report.CustomerId);
                    if (customer == null)
                    {
                        Console.WriteLine($"Customer with id {report.CustomerId} not found. Passing to DeadLetterQueue" );

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        Thread.Sleep(1000);
                        return;
                    }
                    if (contactInsurance)
                    {
                        List<CustomerContact> insurrances = customer.Contacts.Where(c => c.ContactType == "Insurance").ToList();// (customer.CustomerId);
                        foreach (var insurance in insurrances)
                        {
                            report.Recievers.Add(new ReportReciever
                            {
                                RecieverId = insurance.CustomerContactId,
                                RecieverName = insurance.ContactName,
                                RecieverEmail = insurance.RecieverEmail,
                              
                            });
                        }
                        // Contact insurance
                    }

                    report.Recievers.Add(new ReportReciever
                    {
                        RecieverId = customer.CustomerId,
                        RecieverName = customer.CustomerName,
                        RecieverEmail = customer.CustomerEmail,
                    });

                    foreach (var reciever in report.Recievers)
                    {
                        Console.WriteLine($"Added email: {reciever.RecieverEmail} reciever:{reciever.RecieverName}");
                        Thread.Sleep(1000);
                    }

                    // Enrich the report
                   // report.Title = "Enriched " + report.Title;
                    RedirectToRouter(report);
                }
                //Console.WriteLine("Received {0}", message);
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            string consumerTag = channel.BasicConsume(queueName, false, consumer);

            Console.ReadLine();

            channel.BasicCancel(consumerTag);
            channel.Close();
            cnn.Close();
        }

        public static void RedirectToRouter(Report report)
        {
            // Send messages to the next service
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new System.Uri("amqp://guest:guest@localhost:5672");
            factory.ClientProvidedName = "Enricher sender app";

            IConnection cnn = factory.CreateConnection();

            IModel channel = cnn.CreateModel();
            string exchangeName = "ContentRouterExchange";
            string queueName = "ContentRouterQueue";
            string routingKey = "ContentRouterRoutingKey";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            string json = JsonConvert.SerializeObject(report);
            byte[] messageBytes = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchangeName, routingKey, null, messageBytes);
            Console.WriteLine("Sent report to contentbased-router");
            Thread.Sleep(1000);
            channel.Close();
            cnn.Close();

        }
    }


    
}
