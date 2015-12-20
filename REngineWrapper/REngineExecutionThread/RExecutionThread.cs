using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;

using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using RDotNet;
using RDotNet.NativeLibrary;
using RDotNet.Devices;
using RDotNet.Utilities;
using RDotNet.Dynamic;

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Messengers;

using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

using System.IO;
using System.Threading;
using System.ComponentModel.Design;

using System.Windows;

using Messages;
using BaseTypes;
using Communications;
using InstanceFactory;

using NFX;
using NFX.Serialization;
using NFX.Serialization.Slim;
using NFX.Serialization.BSON;

using MathNet.Numerics.Random;

using Utilities;

namespace RManaged
{
    public sealed class RExecutionThread : BaseLayer, IDisposable
    {
        //private string EnvironmentChangeScript { get; set; }
        private REngine Engine { get; set; }
        //private RShareClientServer Rclient { get; set; }

        //private HandlerStoreHouse InternalHandlerStore { get; set; }
        /*
        private string CreateEnvironmentChangesScript()
        {
            return string.Format(@"environmentTrace{0} <- track.summary(access = 1); 
                                   variableNames{0} <- attr(environmentTrace{0}, ""row.names"");
                                   modifiedTime{0} <- environmentTrace{0}$modified;

                                   changedVariablesName{0} <- unlist(
                                                                     sapply(1:length(variableNames{0}), 
				   		                                                  
                                                                     function(i) 
                                                                     {{
                                                                        if ((modifiedTime{0}[i] > startTime{0}) && (variableNames{0}[i] != ""startTime{0}"")) 
                                                                        {{
								                                            return (variableNames{0}[i]);
							                                            }}
    	   		                                                     }})
				  	                                                );

                                   changedVariableValues{0} <- sapply(changedVariablesName{0}, function(elem{0}) get(elem{0}));
                                   changedVariableList{0} <- setNames(as.list(changedVariableValues{0}), changedVariablesName{0})

                                   serializedList{0} <- serialize(changedVariableList{0}, NULL);", InternalRandomIdentifier);
        }

        */
        /*
        private byte [] CreateRSerializedAnswer(SymbolicExpression dataToRSerialize)
        {
            var rawDataName = string.Format("dataToSerialize{0}", InternalRandomIdentifier);

            Engine.SetSymbol(rawDataName, dataToRSerialize);
            var byteArray = Engine.Evaluate(string.Format("serializedData{0} <- serialize(dataToSerialize{0}, NULL);", InternalRandomIdentifier))
                                           .AsRaw().ToArray();

            return byteArray;
        }
        */
        private void Initialize()
        {
            var log = XElement.Load(LogName);

            Console.WriteLine(LogName);

            var library = log.Descendants("Library").Select(elem => { return elem.Value; }).ToList();

            var envPath = Environment.GetEnvironmentVariable("PATH");
            var RBinPath = log.Descendants("RBinPath").Select(elem => { return elem.Value; }).ElementAt(0);
            var RHomePath = log.Descendants("RHomePath").Select(elem => { return elem.Value; }).ElementAt(0);
            var CSharpServerAddress = log.Descendants("CSharpAddress").Select(elem => { return elem.Value; }).ElementAt(0);
            var CSharpServerPort = int.Parse(log.Descendants("CSharpPort").Select(elem => { return elem.Value; }).ElementAt(0));
            //var RServerAddress = log.Descendants("RAddress").Select(elem => { return elem.Value; }).ElementAt(0);
            //var RServerPort = int.Parse(log.Descendants("RPort").Select(elem => { return elem.Value; }).ElementAt(0));

            REngine.SetEnvironmentVariables(RBinPath, RHomePath);
            Engine = REngine.GetInstance();

            #region R Initialize

            Engine.Initialize();

            Engine.Evaluate(@"rm(list = ls(all = TRUE));");

            library.ForEach(elem => { Engine.Evaluate(string.Format(@"library({0})", elem)); });

            Engine.Evaluate(string.Format(@"maxThreads{0} <- max(1, detectCores() - 1)", InternalRandomIdentifier));
            Engine.Evaluate(string.Format(@"internalCluster{0} <- makeCluster(maxThreads{0})", InternalRandomIdentifier));

            library.ForEach(elem => { Engine.Evaluate(string.Format(@"clusterEvalQ(internalCluster{0}, {{ library({1}); }})", InternalRandomIdentifier, elem)); });
            library.ForEach(elem => { Engine.Evaluate(string.Format(@"library({0});", elem)); });
            //Engine.Evaluate(@"options(""digits.secs""=10);");

            //Engine.Evaluate("track.stop();");
            //Engine.Evaluate(string.Format(@"track.start(auto = TRUE, clobber = ""vars"", dir = file.path('D:/TempDataCollector', 'rdatadir{0}'));", InternalRandomIdentifier));

            //Rclient = new RShareClientServer(Engine);
            //Rclient.ServerRshareInitialize(RServerAddress, RServerPort);//, "client.only = TRUE");

            #endregion

            ///throw new NotImplementedException();

            //EnvironmentChangeScript = CreateEnvironmentChangesScript();

            Сonnector = new TCPClient(CSharpServerAddress, CSharpServerPort);

            //this.Сonnector.Serializer = Jil.Serialize.

            //InternalHandlerStore = new HandlerStoreHouse();

            //InternalHandlerStore.Add(Execute, new[] { typeof(IMethodCall), typeof(IDataCollection<object>), typeof(IAnswerable) });
            //InternalHandlerStore.Add(EnvironmentWide, new[] { typeof(IEnvironment), typeof(IDataCollection<byte []>) });

            //Сonnector.MessageReceived += InternalHandlerStore.EntryPoint;

            this.Сonnector.MessageReceived += ((sender, args) => 
                                                {
                                                    var incomingMessage = ((TransferMessageWrapper)args).Message;
                                                    
                                                    Execute(incomingMessage);
                                                    EnvironmentWide(incomingMessage);
                                                });
        }

        private void EnvironmentWide(BaseMessage message)
        {
            Console.WriteLine("Got new Environment wide message");

            var incomingMessage = message as EnvironmentWideMessage;

            if (incomingMessage != null)
            {
                /*
                var rawRVector = Engine.CreateRawVector(incomingMessage.GetData().First());
                Engine.SetSymbol(string.Format("serializedEnvironmentData{0}", InternalRandomIdentifier), rawRVector);

                Engine.Evaluate(string.Format(@"incomedData{0} <- unserialize(serializedEnvironmentData{0});
                                                sapply(names(incomedData{0}), 
                                                function(elem{0}) {{ assign(elem{0}, incomedData{0}[[elem{0}]], envir = .GlobalEnv); }})",
                                                InternalRandomIdentifier));

                Console.WriteLine("incomed environment:");
                Engine.Evaluate(string.Format(@"print(incomedData{0})", InternalRandomIdentifier));
                */

                Engine.SetRenewedEnvironment(incomingMessage.GetData().First(), InternalRandomIdentifier);

            }

            //deserializedList{0} <- unserialize(serializedData{0});

            //changedVariableValues{ 0} < -sapply(changedVariablesName{ 0}, function(elem{ 0}) get(elem{ 0}));
            //changedVariableList{ 0} < -setNames(as.list(changedVariableValues{ 0}), changedVariablesName{ 0})

            //serializedList{ 0} < -serialize(changedVariableList{ 0}, NULL); ", InternalRandomIdentifier);
            Engine.PrintEnvironmentNames();
        }

        private void Execute(BaseMessage message)
        {
            //Rclient.GetSharedEnvironment();

            var incomingMessage = message as MethodCallMessage;

            if (incomingMessage != null)
            {
                Console.WriteLine("Message Received. Type: MethodCallMesage");

                //Engine.Evaluate(string.Format(@"startTime{0} <- as.POSIXlt(Sys.time());", InternalRandomIdentifier));

                var method = Engine.GetType().GetMethod(incomingMessage.GetMethod(), incomingMessage.GetData().Select(elem => { return elem.GetType(); }).ToArray());
                var result = method.Invoke(Engine, incomingMessage.GetData().ToArray());

                //Console.WriteLine("Method invoked");

                var attributes = incomingMessage.MethodAttributes;
                
                if (attributes.Contains(typeof(AnswerableAttribute)))
                {
                    //Console.WriteLine("Method is Answerable, sending result");

                    var asMultipleInstance = result as ICollection<SymbolicExpression>; //bottleneck, cannot work with collection of elements which not derived form SymbolicExpression
                    List<byte[]> serializedList = new List<byte[]>();

                    if (asMultipleInstance == null)
                    {
                        serializedList.Add(Engine.SerializeRObject((SymbolicExpression)result, InternalRandomIdentifier));
                    }
                    else
                        asMultipleInstance.ForEach(elem => serializedList.Add(Engine.SerializeRObject((SymbolicExpression)result, InternalRandomIdentifier)));

                    //var environmentChange = CreateRSerializedAnswer(Engine.Evaluate(EnvironmentChangeScript));


                    //var packingScript = string.Format(@"changedVariablesName{0} <- ls(globalenv());
                    //                                    changedVariableValues{0} <- sapply(changedVariablesName{0}, function(elem{0}) get(elem{0}));
                    //                                    changedVariableList{0} <- setNames(as.list(changedVariableValues{0}), changedVariablesName{0});", 
                    //                                    InternalRandomIdentifier);

                    Console.WriteLine("Creating environment");

                    var environment = Engine.GetEnvironmentList(InternalRandomIdentifier, ".GlobalEnv", RHelper.RSystemWidedInternals);

                    Console.WriteLine("Packing environment");
                    var serializedEnvironment = Engine.SerializeRObject(environment, InternalRandomIdentifier);

                    //deserializedList{0} <- unserialize(serializedData{0});

                    //changedVariableValues{ 0} < -sapply(changedVariablesName{ 0}, function(elem{ 0}) get(elem{ 0}));
                    //changedVariableList{ 0} < -setNames(as.list(changedVariableValues{ 0}), changedVariablesName{ 0})

                    //serializedList{ 0} < -serialize(changedVariableList{ 0}, NULL); ", InternalRandomIdentifier);

                    var dataTosend = new Dictionary<string, ICollection<byte[]>>();
                    dataTosend.Add("result", serializedList);
                    dataTosend.Add("environment", new[] { serializedEnvironment });

                    Сonnector.SendMessage(new AnswerMessage(dataTosend, true));//, new byte[][] { environmentChange }));

                }

                Engine.PrintEnvironmentNames();
            }
                //Rclient.SetSharedEnvironment();
        }
        public RExecutionThread(string logName) : base()
        {
            this.LogName = logName;

            Initialize();
        }
        public void Dispose()
        {
            Engine.Dispose();
            ((TCPClient)Сonnector).Dispose();
        }
    }
}
