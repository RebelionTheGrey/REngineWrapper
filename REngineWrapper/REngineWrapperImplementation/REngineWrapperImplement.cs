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

using System.Diagnostics;
using System.CodeDom;
using System.CodeDom.Compiler;

using RDotNet;
using RDotNet.NativeLibrary;

using Microsoft.CSharp;

using RManaged.BaseTypes;
using RManaged.Communications;

using System.Runtime.InteropServices;

using RManaged.Utilities;
using RManaged.Extensions;


namespace RManaged.Core
{
    using Proxies;

    public sealed partial class REngineWrapper : BaseLayer, IREngine
    {
        private ConcurrentDictionary<long, bool> clientID;
        private SemaphoreSlim semaphores;

        private List<Process> clientProcess;
        public REngine DecoratedEngine { get; private set; }

        private void RenewEnvironment(BaseMessage message)
        {
            var answerMessage = message as AnswerMessage;

            if (answerMessage != null)
            {
                var serializedREnvironment = answerMessage.SerializedEnvironment.ElementAt(0);
                DecoratedEngine.SetRenewedEnvironment(serializedREnvironment, InternalRandomIdentifier);
            }
        }
        private Tuple<List<SymbolicExpression>, byte []> ParseAnswerMessage(BaseMessage message)
        {
            var answerMessage = message as AnswerMessage;

            if (answerMessage != null)
            {
                var serializedRObjects = answerMessage.SerializedResult;
                var serializedREnvironment = answerMessage.SerializedEnvironment.ElementAt(0);

                List<SymbolicExpression> parsedRData = new List<SymbolicExpression>();

                foreach (var elem in serializedRObjects)
                {
                    parsedRData.Add(DecoratedEngine.DeserializeRObject(elem, InternalRandomIdentifier));
                }

                return new Tuple<List<SymbolicExpression>, byte[]>(parsedRData, serializedREnvironment);
            }
            else
                return null;
        }
        public static IREngine GetWrapperInstance(string configName)
        {
            IREngine wrapper = new REngineWrapper(configName);

            Func<IREngine, string, object[], EmitProxyExecute<IREngine>, object> InterceptorFunc =
                ((objectToInvoke, methodName, parameters, execute) =>
                {
                    var castedObject = (REngineWrapper)objectToInvoke;
                    var executionResult = castedObject.Executor(objectToInvoke, methodName, parameters, execute);

                    return executionResult;
                });

            return wrapper.Proxy<IREngine>((methodInfo) => { return !methodInfo.Name.Equals("get_DecoratedEngine"); }, 
                                            new EmitProxyInterceptor<IREngine>(InterceptorFunc));
        }

        private REngineWrapper(string configName)
        {
            this.ConfigName = configName;

            Initialize();
        }
        private void Initialize()
        {
            var config = XElement.Load(ConfigName);            

            var CSharpServerAddress = config.Descendants("CSharpAddress").Select(elem => { return elem.Value; }).ElementAt(0);
            var CSharpServerPort = int.Parse(config.Descendants("CSharpPort").Select(elem => { return elem.Value; }).ElementAt(0));
            
            var clientCount = int.Parse(config.Descendants("ClientCount").Select(elem => { return elem.Value; }).ElementAt(0));
            var clientConfigFile = config.Descendants("ClientConfigFile").Select(elem => { return elem.Value; }).ElementAt(0);

            var envPath = Environment.GetEnvironmentVariable("PATH");
            var RBinPath = config.Descendants("RBinPath").Select(elem => { return elem.Value; }).ElementAt(0);
            var RHomePath = config.Descendants("RHomePath").Select(elem => { return elem.Value; }).ElementAt(0);
            var PreloadScriptName = config.Descendants("PreloadServerScriptName").Select(elem => { return elem.Value; }).ElementAt(0);

            var RExecutionProcess = config.Descendants("RExecutionProcess");
            var RExecutionProcessName = RExecutionProcess.Descendants("Name").Select(elem => { return elem.Value; }).ElementAt(0);
            var RExecutionProcessSource = RExecutionProcess.Descendants("Source").Select(elem => { return elem.Value; }).ElementAt(0);
            var RExecutionProcessLibraries = RExecutionProcess.Descendants("Libraries").Select(elem => { return elem.Value; });

            #region R Initialize

            REngine.SetEnvironmentVariables(RBinPath, RHomePath);
            DecoratedEngine = REngine.GetInstance();

            DecoratedEngine.Initialize();

            var preloadScript = new Script(PreloadScriptName, null);
            DecoratedEngine.Evaluate(preloadScript.ScriptBody);

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

            clientProcess = new List<Process>();

            
            var compiler = new CSharpCodeProvider();
            var parameters = new CompilerParameters(RExecutionProcessLibraries.ToArray(), RExecutionProcessName, true);
            parameters.CompilerOptions = "/platform:x64";
            parameters.GenerateExecutable = true;

            CompilerResults results = compiler.CompileAssemblyFromFile(parameters, RExecutionProcessSource);

            for (int i = 0; i < clientCount; i++)
            {
                var newProcess = new Process();
                newProcess.StartInfo.FileName = results.CompiledAssembly.CodeBase;
                newProcess.StartInfo.Arguments = clientConfigFile;

                clientProcess.Add(newProcess);

                newProcess.Start();
            }

            Console.ReadLine();
        }
    }
}
