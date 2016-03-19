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
    public static class SerializerFabric
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
            var binarySerializer = new BinaryFormatter();
            binarySerializer.FilterLevel = TypeFilterLevel.Low;

            return binarySerializer;
        }
    }

    public abstract class CommunicationProtocol : IDisposable
    {
        protected Stream stream;

        public string Address { get; protected set; }
        public int Port { get; protected set; }

        public event MessageReceiveHandler MessageReceived;
        public event MessageReceiveHandler MessageSent;

        protected dynamic Serializer { get; set; }

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

        #region IDisposable Support
        protected bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                // TODO: задать большим полям значение NULL.

                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
        ~CommunicationProtocol()
        {
          // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            Dispose(false);
        }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            Dispose(true);
            // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
             GC.SuppressFinalize(this);
        }
        #endregion
    }

    public sealed class TCPClient : CommunicationProtocol
    {
        private IScsClient TcpClient { get; set; }

        protected override void ReceiveMessageHandler(object messageSender, EventArgs messageArgs)
        {
            var receivedData = ((MessageEventArgs)messageArgs).Message as ScsRawDataMessage;
            stream.Write(receivedData.MessageData, 0, receivedData.MessageData.Length);

            var deserializedMessage = (BaseMessage)Serializer.Deserialize(stream);
            OnMessageReceived(deserializedMessage);

            stream.Seek(0, SeekOrigin.Begin);

            //Action<BaseMessage> ExecuteHandlers = (BaseMessage message) => base.OnMessageReceived(message);
            //ExecuteHandlers(deserializedMessage);
        }        
        public override void SendMessage(BaseMessage message, [Optional] IDictionary<string, object> parameters)
        {
            Serializer.Serialize(stream, message);
            TcpClient.SendMessage(new ScsRawDataMessage((stream as MemoryStream).ToArray()));
        }

        public TCPClient(string address, int port): base()
        {
            this.Address = address;
            this.Port = port;

            Initialize();
        }

        private void Initialize()
        {
            stream = new MemoryStream();
            Serializer = SerializerFabric.GetDefaultInstance();

            TcpClient = ScsClientFactory.CreateClient(new ScsTcpEndPoint(Address, Port));
            TcpClient.MessageReceived += ReceiveMessageHandler;

            TcpClient.Connect();
        }

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TcpClient.Disconnect();
                    TcpClient.Dispose();
                }
            }
        }

        #endregion
    }

    public sealed class TCPServer : CommunicationProtocol
    {
        private IScsServer TcpServer { get; set; }
        private IDictionary<long, SynchronizedMessenger<IScsServerClient>> ClientMessengers { get; set; }

        public event MessageReceiveHandler ClientConnected;
        public event MessageReceiveHandler ClientDisconnected;

        protected override void ReceiveMessageHandler(object messageSender, EventArgs messageArgs)
        {
            var receivedData = ((MessageEventArgs)messageArgs).Message as ScsRawDataMessage;
            stream.Write(receivedData.MessageData, 0, receivedData.MessageData.Length);

            var deserializedMessage = (BaseMessage)Serializer.Deserialize(stream);
            OnMessageReceived(deserializedMessage);

            stream.Seek(0, SeekOrigin.Begin);

            //Action<BaseMessage> ExecuteHandlers = (BaseMessage message) => base.OnMessageReceived(message);
            //ExecuteHandlers(temporaryMessage);
        }

        public override void SendMessage(BaseMessage message, [Optional] IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey("clientID"))
                {
                    var client = TcpServer.Clients.GetAllItems().First(elem => elem.ClientId == (long)parameters["clientID"]);

                    Serializer.Serialize(stream, message);
                    client.SendMessage(new ScsRawDataMessage((stream as MemoryStream).ToArray()));
                }
            }
        }

        public override BaseMessage ReceiveMessage([Optional] IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey("clientID"))
                {
                    var receivedData = ClientMessengers[(long)parameters["clientID"]].ReceiveMessage<ScsRawDataMessage>();
                    stream.Write(receivedData.MessageData, 0, receivedData.MessageData.Length);

                    var deserializedMessage = (BaseMessage)Serializer.Deserialize(stream);
                    OnMessageReceived(deserializedMessage);

                    stream.Seek(0, SeekOrigin.Begin);

                    return (BaseMessage)Serializer.Deserialize(stream);
                }
            }

            return new EmptyMessage();
        }

        public override Task SendMessageAsync(BaseMessage message, [Optional] IDictionary<string, object> parameters) 
        {
            return Task.Run(() => SendMessage(message, parameters));
        }
        public override Task<BaseMessage> ReceiveMessageAsync([Optional] IDictionary<string, object> parameters) 
        {
            return Task.Run(() => ReceiveMessage(parameters));
        }

        public void MulticastSendMessage(BaseMessage message, [Optional] ICollection<long> exceptClientIDs)
        {
            var allClients = TcpServer.Clients.GetAllItems();
            var appropriateClients = exceptClientIDs != null ? allClients : allClients.Where(elem => !exceptClientIDs.Contains(elem.ClientId));

            Serializer.Serialize(stream, message);
            appropriateClients.AsParallel().ForAll(elem => { elem.SendMessage(new ScsRawDataMessage((stream as MemoryStream).ToArray())); });
        }
        public Task MulticastSendMessageAsync(BaseMessage message, [Optional] ICollection<long> exceptClientIDs)
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

        private void Connected(object messageSender, EventArgs messageArgs)
        {
            MessageReceiveHandler handler = ClientConnected;
            var client = ((ServerClientEventArgs)messageArgs).Client;

            var synchronizedMessenger = new SynchronizedMessenger<IScsServerClient>(client);
            synchronizedMessenger.Start();

            ClientMessengers.Add(client.ClientId, synchronizedMessenger);

            if (handler != null)
                handler(messageSender, messageArgs);
        }
        private void Disconnected(object messageSender, EventArgs messageArgs)
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

        private void Initialize()
        {
            ClientMessengers = new ConcurrentDictionary<long, SynchronizedMessenger<IScsServerClient>>();

            TcpServer = ScsServerFactory.CreateServer(new ScsTcpEndPoint(Address, Port));           

            TcpServer.ClientConnected += Connected;
            TcpServer.ClientDisconnected += Disconnected;

            TcpServer.Start();
        }

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TcpServer.Clients.ClearAll();
                    TcpServer.Stop();
                }
            }
        }

        #endregion
    }
}
