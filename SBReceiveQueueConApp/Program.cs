using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBReceiveQueueConApp
{
    class Program
    {
        static NamespaceManager _namespaceManager;
        static List<BrokeredMessage> _messageList;
        static void Main(string[] args)
        {
            CollectSBDetails();
            CreateQueueToRead();
            Console.Read();
        }

        static void CreateQueueToRead()
        {
            TokenProvider tokenProvider = _namespaceManager.Settings.TokenProvider;
            if (_namespaceManager.QueueExists("categoryqueue"))
            {
                MessagingFactory factory = MessagingFactory.Create(_namespaceManager.Address, tokenProvider);
                QueueClient catsQueueclient = factory.CreateQueueClient("categoryqueue");
                Console.WriteLine("Receiving the message from the queue...");
                BrokeredMessage message;
                int ctr = 1;
                while ((message =catsQueueclient.Receive(new TimeSpan(hours:0,minutes:1,seconds:5)))!=null)
                {
                    Console.WriteLine($"Message received,Sequence:{message.SequenceNumber}, MessageID :{message.MessageId},\nCat:{ message.Properties[(ctr++).ToString()]}");
                    message.Complete();
                    Console.WriteLine("processing Message(Sleeping)...");
                    Thread.Sleep(2000);
                }
                factory.Close();
                catsQueueclient.Close();
                _namespaceManager.DeleteQueue("categoryqueue");
                Console.WriteLine("Finished getting alll the data from the queue ,Press any key to exit");
            }
        }
        static void CollectSBDetails()
        {
            _namespaceManager = NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"].ToString());

        }
    }
}
