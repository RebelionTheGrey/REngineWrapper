using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using System.Runtime.Serialization;

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Communication;

using System.Reflection;
using System.Reflection.Emit;

using RDotNet;
using RDotNet.NativeLibrary;

//using Qoollo.Turbo.Threading;


using BaseTypes;
using Communications;
using Proxies;
using Messages;

using System.Runtime.InteropServices;

using Utilities;

namespace RManaged.Core
{
    public sealed partial class REngineWrapper : BaseLayer, IREngine
    {
        private ConcurrentDictionary<long, bool> clientID;
        private SemaphoreSlim semaphores;

        //private RShareClientServer RServer;

        private REngine decoratedEngine;
        public REngine DecoratedEngine { get { return decoratedEngine; } }
        //private SemaphoreLight lightSemaphores;

        private void RenewEnvironment(BaseMessage message)
        {
            var answerMessage = message as AnswerMessage;

            if (answerMessage != null)
            {
                //var serializedRObjects = answerMessage.GetData("result");
                var serializedREnvironment = answerMessage.GetData("environment").ElementAt(0);

                //DecoratedEngine.Evaluate(string.Format(@"incomedData{0} <- unserialize(serializedList{0});
                //                                         sapply(names(incomedData{0}), 
                //                                         function(elem{0}) {{ assign(elem{0}, incomedData{0}[[elem{0}]]); }})", 
                //                                         InternalRandomIdentifier));

                DecoratedEngine.SetRenewedEnvironment(serializedREnvironment, InternalRandomIdentifier);
            }
        }
        private Tuple<List<SymbolicExpression>, byte []> ParseAnswerMessage(BaseMessage message)
        {
            var answerMessage = message as AnswerMessage;

            if (answerMessage != null)
            {
                var serializedRObjects = answerMessage.GetData("result");
                var serializedREnvironment = answerMessage.GetData("environment").ElementAt(0);

                List<SymbolicExpression> parsedRData = new List<SymbolicExpression>();

                foreach (var elem in serializedRObjects)
                {
                    //DecoratedEngine.SetSymbol(string.Format("objectToDeserialize{0}", InternalRandomIdentifier), new RawVector(DecoratedEngine, elem));
                    //DecoratedEngine.Evaluate(string.Format("deserializedRObject{0} <- unserialize(objectToDeserialize{0});", InternalRandomIdentifier));
                    //parsedRData.Add(DecoratedEngine.GetSymbol(string.Format("deserializedRObject{0}", InternalRandomIdentifier)));
                    parsedRData.Add(DecoratedEngine.DeserializeRObject(elem, InternalRandomIdentifier));
                }

                //DecoratedEngine.SetSymbol(string.Format("serializedList{0}", InternalRandomIdentifier), new RawVector(DecoratedEngine, serializedREnvironment));
                //DecoratedEngine.Evaluate(string.Format(@"incomedEnvironment{0} <- unserialize(serializedList{0});
                //                                         sapply(names(incomedEnvironment{0}), 
                //                                         function(elem{0}) {{ assign(elem{0}, incomedEnvironment{0}[[elem{0}]]); }})",
                //                                         InternalRandomIdentifier));

                return new Tuple<List<SymbolicExpression>, byte[]>(parsedRData, serializedREnvironment);
            }
            else
                return null;
        }
        public static IREngine GetWrapperInstance(string logName)
        {
            IREngine wrapper = new REngineWrapper(logName);

            Func<IREngine, string, object[], EmitProxyExecute<IREngine>, object> InterceptorFunc =
                ((objectToInvoke, methodName, parameters, execute) =>
                {
                    var castedObject = (REngineWrapper)objectToInvoke;
                    var executionResult = castedObject.Executor(objectToInvoke, methodName, parameters, execute);

                    return executionResult;
                });

            //return wrapper.Proxy<IREngine>(new EmitProxyInterceptor<IREngine>(InterceptorFunc));
            return wrapper.Proxy<IREngine>((methodInfo) => { return !methodInfo.Name.Equals("get_DecoratedEngine"); }, 
                                            new EmitProxyInterceptor<IREngine>(InterceptorFunc));
        }
        /*
        private byte[] CreateRSerializedAnswer(SymbolicExpression dataToRSerialize)
        {
            var rawDataName = string.Format("dataToSerialize{0}", InternalRandomIdentifier);

            DecoratedEngine.SetSymbol(rawDataName, dataToRSerialize);
            var byteArray = DecoratedEngine.Evaluate(string.Format("serializedData{0} <- serialize(dataToSerialize{0}, NULL);", InternalRandomIdentifier))
                                           .AsRaw().ToArray();

            return byteArray;
        }
        */
        private REngineWrapper(string LogName)
        {
            this.LogName = LogName;

            Initialize();
        }
        private void Initialize()
        {
            var log = XElement.Load(LogName);
            

            var CSharpServerAddress = log.Descendants("CSharpAddress").Select(elem => { return elem.Value; }).ElementAt(0);
            var CSharpServerPort = int.Parse(log.Descendants("CSharpPort").Select(elem => { return elem.Value; }).ElementAt(0));

            //var RServerAddress = log.Descendants("RAddress").Select(elem => { return elem.Value; }).ElementAt(0);
            //var RServerPort = int.Parse(log.Descendants("RPort").Select(elem => { return elem.Value; }).ElementAt(0));


            var library = log.Descendants("Library").Select(elem => { return elem.Value; }).ToList();

            var envPath = Environment.GetEnvironmentVariable("PATH");
            var RBinPath = log.Descendants("RBinPath").Select(elem => { return elem.Value; }).ElementAt(0);
            var RHomePath = log.Descendants("RHomePath").Select(elem => { return elem.Value; }).ElementAt(0);

            REngine.SetEnvironmentVariables(RBinPath, RHomePath);
            decoratedEngine = REngine.GetInstance();

            #region R Initialize

            DecoratedEngine.Initialize();

            DecoratedEngine.Evaluate(@"rm(list = ls(all = TRUE));");

            library.ForEach(elem => { DecoratedEngine.Evaluate(string.Format(@"library({0})", elem)); });

            //DecoratedEngine.Evaluate(@"maxThreads <- max(1, detectCores() - 1)");
            //DecoratedEngine.Evaluate(@"internalCluster <- makeCluster(maxThreads)");

            //library.ForEach(elem => { DecoratedEngine.Evaluate(string.Format(@"clusterEvalQ(internalCluster, {{ library({0}); }})", elem)); });

            //DecoratedEngine.Evaluate("options(\"digits.secs\"=10);");

            //DecoratedEngine.Evaluate("track.stop();");
            //DecoratedEngine.Evaluate("track.start(clobber = \"vars\");");

            //RServer = new RShareClientServer(DecoratedEngine);
            //RServer.ServerRshareInitialize(RServerAddress, RServerPort);//, "server.only = TRUE");

            //DecoratedEngine.Evaluate("getStatus(7777)");

            #endregion




            Сonnector = new TCPServer(CSharpServerAddress, CSharpServerPort);

            ((TCPServer)Сonnector).ClientConnected += ((object messageSender, EventArgs args) =>
                                                      {
                                                            var derivedMessage = (ServerClientEventArgs)args;
                                                            var client = derivedMessage.Client;

                                                            clientID.TryAdd(client.ClientId, true);
                                                            semaphores.Release(1);
                                                      });

            ((TCPServer)Сonnector).ClientDisconnected += (async (object messageSender, EventArgs args) =>
                                                         {
                                                             var derivedMessage = (ServerClientEventArgs)args;
                                                             var client = derivedMessage.Client;

                                                             bool outValue;
                                                             if (clientID.TryRemove(client.ClientId, out outValue))
                                                                 await semaphores.WaitAsync();
                                                         });

            clientID = new ConcurrentDictionary<long, bool>();
            semaphores = new SemaphoreSlim(0, 100);

            //EnvironmentWideScript = CreateEnvironmentWideScript();

            //lightSemaphores = new SemaphoreLight(0);
        }
    }
}
