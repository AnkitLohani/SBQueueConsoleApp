using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;

namespace SBQueueConsoleApp
{
    class Program
    {
        static NamespaceManager _namespaceManager;
        static List<BrokeredMessage> _messageList;
        static void Main(string[] args)
        {
            dynamic categories = GetJsonArray();
            _messageList = GenerateMessages(categories);
            Console.WriteLine($"MessageList.Count -> {_messageList.Count}");
            CollectSBDetails();
            CreateQueue(_messageList.Count);
            Console.ReadKey(true);
        }

        static void CreateQueue(int messagecount)
        {
            QueueDescription catsQueue = null;
            if (!_namespaceManager.QueueExists("categoryqueue"))
            {
                catsQueue = _namespaceManager.CreateQueue("categoryQueue");
            }
            MessagingFactory factory = MessagingFactory.Create(_namespaceManager.Address, _namespaceManager.Settings.TokenProvider);
            QueueClient catsQueueClient = factory.CreateQueueClient("categoryqueue");
            Console.WriteLine("Sending message to the queue...");
            for (int i = 0; i < messagecount; i++)
            {
                var cat = _messageList[i];
                cat.Label = cat.Properties[(i + 1).ToString()].ToString();
                catsQueueClient.Send(cat);
                Console.WriteLine($"Message Id: {cat.MessageId},Message Sent: {cat.Label}");
            }
        
        }
        static void CollectSBDetails()
        {
            _namespaceManager=NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings[ "Microsoft.ServiceBus.ConnectionString"].ToString());
                
        }
        static List<BrokeredMessage> GenerateMessages(dynamic categories)
        {
            List<BrokeredMessage> result = new List<BrokeredMessage>();
            foreach (var item in categories)
            {
                BrokeredMessage message = new BrokeredMessage();
                message.Properties.Add(item.CategoryID.ToString(), item.ToString());
                result.Add(message);
            }
            return result;
        }
        static JArray GetJsonArray()
        {
            string jsonData = null;
            using (StreamReader reader = new StreamReader(@"../../Categories.txt"))
            {
                jsonData = reader.ReadToEnd();
            }
            JArray jArrayData = JArray.Parse(jsonData);
            return jArrayData;

        }
    }
}
