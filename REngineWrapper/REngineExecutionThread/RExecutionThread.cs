﻿using System;
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

using RManaged.BaseTypes;
using RManaged.Communications;

using NFX;
using NFX.Serialization;
using NFX.Serialization.Slim;
using NFX.Serialization.BSON;

using MathNet.Numerics.Random;

using RManaged.Utilities;

//internalCluster is a cluster system which could be used as a default cluster in any parallel code execution


namespace RManaged.Core
{
    public sealed class RExecutionThread : BaseLayer, IDisposable
    {
        private REngine Engine { get; set; }
        private void Initialize()
        {
            var config = XElement.Load(ConfigName);

            Console.WriteLine(ConfigName);

            var envPath = Environment.GetEnvironmentVariable("PATH");
            var RBinPath = config.Descendants("RBinPath").Select(elem => { return elem.Value; }).ElementAt(0);
            var RHomePath = config.Descendants("RHomePath").Select(elem => { return elem.Value; }).ElementAt(0);
            var CSharpServerAddress = config.Descendants("CSharpAddress").Select(elem => { return elem.Value; }).ElementAt(0);
            var CSharpServerPort = int.Parse(config.Descendants("CSharpPort").Select(elem => { return elem.Value; }).ElementAt(0));
            var PreloadScriptName = config.Descendants("PreloadServiceScriptName").Select(elem => { return elem.Value; }).ElementAt(0);

            #region R Initialize

            REngine.SetEnvironmentVariables(RBinPath, RHomePath);
            Engine = REngine.GetInstance();

            Engine.Initialize();

            var preloadScript = new Script(PreloadScriptName);
            Engine.Evaluate(preloadScript.ScriptBody);

            #endregion

            Сonnector = new TCPClient(CSharpServerAddress, CSharpServerPort);

            this.Сonnector.MessageReceived += ((sender, args) => 
                                                {
                                                    var incomingMessage = ((TransferMessageWrapper)args).Message;
                                                    
                                                    Execute(incomingMessage);
                                                    EnvironmentWide(incomingMessage);
                                                });
        }

        private void EnvironmentWide(BaseMessage message)
        {
            var incomingMessage = message as EnvironmentWideMessage;

            if (incomingMessage != null)
            {
                Engine.SetRenewedEnvironment(incomingMessage.Data.First(), InternalRandomIdentifier);
            }
        }
        private void Execute(BaseMessage message)
        {
            var incomingMessage = message as MethodCallMessage;

            if (incomingMessage != null)
            {
                var method = Engine.GetType().GetMethod(incomingMessage.MethodName, incomingMessage.Parameters.Select(elem => { return elem.GetType(); }).ToArray());
                var result = method.Invoke(Engine, incomingMessage.Parameters.ToArray());

                var attributes = incomingMessage.MethodAttributes;
                
                if (attributes.Contains(typeof(AnswerableAttribute)))
                {
                    var asMultipleInstance = result as ICollection<SymbolicExpression>;
                    List<byte[]> serializedList = new List<byte[]>();

                    if (asMultipleInstance == null)
                    {
                        serializedList.Add(Engine.SerializeRObject((SymbolicExpression)result, InternalRandomIdentifier));
                    }
                    else
                        asMultipleInstance.ForEach(elem => serializedList.Add(Engine.SerializeRObject((SymbolicExpression)result, InternalRandomIdentifier)));

                    var environment = Engine.GetEnvironmentList(InternalRandomIdentifier, RHelper.GlobalEnvironmentName, RHelper.RSystemWidedInternals);
                    var serializedEnvironment = new[] { Engine.SerializeRObject(environment, InternalRandomIdentifier) };
                    
                    Сonnector.SendMessage(new AnswerMessage(serializedList, serializedEnvironment, true));
                }
            }
        }
        public RExecutionThread(string logName) : base()
        {
            this.ConfigName = logName;

            Initialize();
        }

        #region IDisposable Support
        private bool disposedValue = false; 

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Engine.Dispose();
                    ((TCPClient)Сonnector).Dispose();
                }

                disposedValue = true;
            }
        }
         ~RExecutionThread()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
