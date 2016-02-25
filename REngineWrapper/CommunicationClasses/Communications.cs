using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

using System.Runtime.InteropServices;
using System.Reflection;

using System.Text;
using System.IO;


using RManaged.BaseTypes;

using NFX.Serialization.Slim;


using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using RDotNet;
using RDotNet.Utilities;


namespace RManaged.Communications
{
    public class PrimitiveSerializerFabric
    {
        public static dynamic GetInstance(string serializerType, [Optional] ICollection<object> parameters)
        {
            switch (serializerType)
            {
                case "NFXSerializer":
                    return new SlimSerializer();
                default:
                    throw new NotImplementedException();
            }
        }
        public static dynamic GetDefaultInstance([Optional] ICollection<object> parameters)
        {
            //var slimSerializer = new SlimSerializer();
            //slimSerializer.TypeMode = TypeRegistryMode.PerCall;

            //return slimSerializer;

            var binarySerializer = new BinaryFormatter();
            binarySerializer.FilterLevel = TypeFilterLevel.Low;

            return binarySerializer;
        }
    }

    public abstract class CommunicationProtocol
    {
        public event MessageReceiveHandler MessageReceived;
        public event MessageReceiveHandler MessageSent;

        protected dynamic Serializer { get; set; } //no better deep polymorphism variant found ((

        protected virtual void OnMessageReceived(BaseMessage message)
        {
            MessageReceiveHandler handler = MessageReceived;

            if (handler != null)
            {
                handler(this, new TransferMessageWrapper(message));
            }
        }
        protected virtual void OnMessageSent(BaseMessage message)
        {
            MessageReceiveHandler handler = MessageSent;

            if (handler != null)
            {
                handler(this, new TransferMessageWrapper(message));
            }
        }

        protected virtual void ReceiveMessageHandler(object messageSender, EventArgs messageArgs) { }
        protected virtual void SendMessageHandler(object messageSender, EventArgs messageArgs) { }


        public virtual void SendMessage(BaseMessage message, [Optional] IDictionary<string, object> parameters) { }
        public virtual BaseMessage ReceiveMessage([Optional] IDictionary<string, object> parameters) { return null; }


        public virtual Task SendMessageAsync(BaseMessage message, [Optional] IDictionary<string, object> parameters) { return Task.Delay(1); }
        public virtual Task<BaseMessage> ReceiveMessageAsync([Optional] IDictionary<string, object> parameters) { return Empty<BaseMessage>.Task; }

        protected virtual void Initialize()
        {
            Serializer = PrimitiveSerializerFabric.GetDefaultInstance();
        }
    }

    public class TCPClient : CommunicationProtocol, IDisposable
    {
        protected virtual IScsClient TcpClient { get; set; }

        public virtual string Address { get; protected set; }
        public virtual int Port { get; protected set; }

        protected override void ReceiveMessageHandler(object messageSender, EventArgs messageArgs)
        {
            var receivedData = (ScsRawDataMessage)((MessageEventArgs)messageArgs).Message;

            using (var stream = new MemoryStream(receivedData.MessageData))
            {
                var temporaryMessage = (BaseMessage)Serializer.Deserialize(stream);
                
                Action<BaseMessage> ExecuteHandlers = (BaseMessage message) => base.OnMessageReceived(message);
                ExecuteHandlers(temporaryMessage);
            }
        }        
        public override void SendMessage(BaseMessage message, [Optional] IDictionary<string, object> parameters)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                TcpClient.SendMessage(new ScsRawDataMessage(stream.ToArray()));
            }
        }

        public TCPClient(string address, int port)
        {
            this.Address = address;
            this.Port = port;

            Initialize();
        }

        public new void Initialize()
        {
            base.Initialize();

            TcpClient = ScsClientFactory.CreateClient(new ScsTcpEndPoint(Address, Port));
            TcpClient.MessageReceived += ReceiveMessageHandler;

            TcpClient.Connect();
        }
        public void Dispose()
        {
            TcpClient.Disconnect();
            TcpClient.Dispose();
        }
    }

    public class TCPServer : CommunicationProtocol, IDisposable
    {
        protected virtual IScsServer TcpServer { get; set; }
        protected virtual IDictionary<long, SynchronizedMessenger<IScsServerClient>> ClientMessengers { get; set; }

        public virtual string Address { get; protected set; }
        public virtual int Port { get; protected set; }


        public event MessageReceiveHandler ClientConnected;
        public event MessageReceiveHandler ClientDisconnected;

        protected override void ReceiveMessageHandler(object messageSender, EventArgs messageArgs)
        {
            var receivedData = (ScsRawDataMessage)((MessageEventArgs)messageArgs).Message;

            using (var stream = new MemoryStream(receivedData.MessageData))
            {
                var temporaryMessage = (BaseMessage)Serializer.Deserialize(stream);

                Action<BaseMessage> ExecuteHandlers = (BaseMessage message) => base.OnMessageReceived(message);
                ExecuteHandlers(temporaryMessage);
            }
        }

        public override void SendMessage(BaseMessage message, [Optional] IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey("clientID"))
                {
                    object clientID = null;
                    parameters.TryGetValue("clientID", out clientID);

                    var client = TcpServer.Clients.GetAllItems().Find(elem => elem.ClientId == (long)clientID);

                    using (var stream = new MemoryStream())
                    {
                        Serializer.Serialize(stream, message);
                        client.SendMessage(new ScsRawDataMessage(stream.ToArray()));
                    }
                }
            }
        }

        public override BaseMessage ReceiveMessage([Optional] IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey("clientID"))
                {
                    object clientID = null;
                    parameters.TryGetValue("clientID", out clientID);

                    var receivedData = ClientMessengers[(long)clientID].ReceiveMessage<ScsRawDataMessage>();

                    using (var stream = new MemoryStream(receivedData.MessageData))
                    {
                        return (BaseMessage)Serializer.Deserialize(stream);
                    }
                }
            }

            return new BaseMessage();
        }

        public override Task SendMessageAsync(BaseMessage message, [Optional] IDictionary<string, object> parameters) 
        {
            return Task.Run(() => SendMessage(message, parameters));
        }
        public override Task<BaseMessage> ReceiveMessageAsync([Optional] IDictionary<string, object> parameters) 
        {
            return Task.Run(() => ReceiveMessage(parameters));
        }

        public virtual void MulticastSendMessage(BaseMessage message, [Optional] ICollection<long> exceptClientIDs)
        {
            var allClients = TcpServer.Clients.GetAllItems();
            var appropriateClients = exceptClientIDs != null ? allClients : allClients.Where(elem => !exceptClientIDs.Contains(elem.ClientId));

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                appropriateClients.AsParallel().ForAll(elem => { elem.SendMessage(new ScsRawDataMessage(stream.ToArray())); });
            }
        }
        public virtual Task MulticastSendMessageAsync(BaseMessage message, [Optional] ICollection<long> exceptClientIDs)
        {
            var allClients = TcpServer.Clients.GetAllItems();
            var appropriateClients = exceptClientIDs.Count == 0 ? allClients : allClients.Where(elem => !exceptClientIDs.Contains(elem.ClientId));

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);

                Action runParallelSendingDelegate = () => { appropriateClients.AsParallel().ForAll(elem => { elem.SendMessage(new ScsRawDataMessage(stream.ToArray())); }); };
                return Task.Run(runParallelSendingDelegate);
            }
        }

        protected virtual void Connected(object messageSender, EventArgs messageArgs)
        {
            //((ServerClientEventArgs)messageArgs).Client.MessageReceived += ReceiveMessageHandler;

            MessageReceiveHandler handler = ClientConnected;
            var client = ((ServerClientEventArgs)messageArgs).Client;

            var synchronizedMessenger = new SynchronizedMessenger<IScsServerClient>(client);
            synchronizedMessenger.Start();

            ClientMessengers.Add(client.ClientId, synchronizedMessenger);

            if (handler != null)
                handler(messageSender, messageArgs);
        }
        protected virtual void Disconnected(object messageSender, EventArgs messageArgs)
        {
            var client = ((ServerClientEventArgs)messageArgs).Client;

            SynchronizedMessenger<IScsServerClient> messenger = null;

            if (ClientMessengers.TryGetValue(client.ClientId, out messenger))
            {
                messenger.Stop();
                messenger.Dispose();
            }

            ClientMessengers.Remove(client.ClientId);
            TcpServer.Clients.Remove(client.ClientId);

            MessageReceiveHandler handler = ClientDisconnected;

            if (handler != null)
                handler(messageSender, messageArgs);
        }

        public TCPServer(string address, int port)
        {
            this.Address = Address;
            this.Port = port;

            Initialize();
        }

        private new void Initialize()
        {
            base.Initialize();

            ClientMessengers = new ConcurrentDictionary<long, SynchronizedMessenger<IScsServerClient>>();

            TcpServer = ScsServerFactory.CreateServer(new ScsTcpEndPoint(Address, Port));           

            TcpServer.ClientConnected += Connected;
            TcpServer.ClientDisconnected += Disconnected;

            TcpServer.Start();
        }
        public void Dispose()
        {
            TcpServer.Clients.ClearAll();
            TcpServer.Stop();
        }
    }

    /*
    public class RShareClientServer
    {
        protected REngine Engine { get; set; }

        public RShareClientServer(REngine engine)
        {
            Engine = engine;
        }

        public void ServerRshareInitialize(string address, int port, params string [] parameters)
        {
            var concatParams = string.Join(", ", parameters);
            Engine.Evaluate(string.Format(@"getStatus({0})", port.ToString()));
            var script = string.Format(string.Format(@"startRshare(port = {0}, {1});", port.ToString(), concatParams));
            Engine.Evaluate(script);
            Engine.Evaluate(string.Format(@"getStatus({0})", port.ToString()));
        }
        public void GetSharedEnvironment()
        {
            var script = string.Format(@"remove(list = ls(globalenv()));
                                         sapply(ls.Rshare(), function(elem) { assign(elem, get.Rshare(elem)); });");
            Engine.Evaluate(script);
        }
        public void SetSharedEnvironment()
        {
            var script = string.Format(@"remove.Rshare(list = ls(globalenv()));
                                         sapply(ls(globalenv()), function(elem) { assign.Rshare(elem, get(elem)); });");
            Engine.Evaluate(script);
        }
    }
    */
}
