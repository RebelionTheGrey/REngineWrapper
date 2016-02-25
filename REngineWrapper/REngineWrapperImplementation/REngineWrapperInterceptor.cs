using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using RManaged.BaseTypes;
using RManaged.Communications;

using RDotNet;
using RDotNet.Internals;

using RManaged.Utilities;


namespace RManaged.Core
{
    using Proxies;

    public sealed partial class REngineWrapper : BaseLayer, IREngine
    {
        private static object locker = new object();
        private object Executor(IREngine objectToInvoke, string methodName, ICollection<object> parameters, EmitProxyExecute<IREngine> execute)
        {
            semaphores.Wait();

            KeyValuePair<long, bool> freeElem;

            lock (locker)
            {
                freeElem = clientID.FirstOrDefault(elem => elem.Value == true);
                clientID[freeElem.Key] = false;
            }

            var methodAttributes = typeof(IREngine).GetMethod(methodName, parameters.Select(elem => { return elem.GetType(); }).ToArray()).
                             GetCustomAttributes(false); //Change on IDictionary realization of fast collection search of methods

            //Dummy code, need to make auxiliary static class of all types.
            var typeCollection = new List<Type>() { typeof(InternalExecutionAttribute), typeof(ExternalExecutionAttribute),
                                                    typeof(AnswerableAttribute), typeof(MulticastExecutionAttribute)};

            IDictionary<Type, bool> attributes = new Dictionary<Type, bool>();

            typeCollection.ForEach(attr =>
            {
                attributes.Add(attr, methodAttributes.Any(elem => elem.GetType().Equals(attr)));
            });

            //make as independent fabric later like a storage, because it's worst code
            //var isInternal = methodAttributes.Any(elem => elem.GetType().Equals(typeof(InternalExecutionAttribute)));
            //var isExternal = methodAttributes.Any(elem => elem.GetType().Equals(typeof(ExternalExecutionAttribute)));
            //var isAnswerable = methodAttributes.Any(elem => elem.GetType().Equals(typeof(AnswerableAttribute)));
            //var isMulticast = methodAttributes.Any(elem => elem.GetType().Equals(typeof(MulticastExecutionAttribute)));
            //var isEnvironmentSwap = methodAttributes.Any(elem => elem.GetType().Equals(typeof(EnvironmentSwapAttribute)));


            //DecoratedEngine.SetSymbol(string.Format("serializedList{0}", InternalRandomIdentifier), new RawVector(DecoratedEngine, serializedREnvironment));
            //DecoratedEngine.Evaluate(string.Format(@"incomedEnvironment{0} <- unserialize(serializedList{0});
            //                                             sapply(names(incomedEnvironment{0}), 
            //                                             function(elem{0}) {{ assign(elem{0}, incomedEnvironment{0}[[elem{0}]]); }})",
            //                                         InternalRandomIdentifier));

            ICollection<SymbolicExpression> resultCollection = null;


            if (attributes.First(elem => elem.Key.Equals(typeof(ExternalExecutionAttribute))).Value)
            {
                var methodValidAttributes = attributes.Where(elem1 => elem1.Value == true).Select(elem2 => { return elem2.Key; }).ToList();
                var requestMessage = new MethodCallMessage(methodName, parameters, methodValidAttributes);

                if (!attributes.First(elem => elem.Key.Equals(typeof(MulticastExecutionAttribute))).Value)
                {
                    var clientParams = new Dictionary<string, object>();
                    clientParams.Add("clientID", freeElem.Key);

                    Сonnector.SendMessage(requestMessage, clientParams);

                    if (attributes.First(elem => elem.Key.Equals(typeof(AnswerableAttribute))).Value)
                    {
                        var incomedMessage = Сonnector.ReceiveMessage(clientParams);
                        var parsedMessage = ParseAnswerMessage(incomedMessage);
                        resultCollection = parsedMessage.Item1;

                        var resultEnvironment = parsedMessage.Item2;

                        //DecoratedEngine.SetSymbol(string.Format("serializedList{0}", InternalRandomIdentifier), DecoratedEngine.CreateRawVector(resultEnvironment));
                        //DecoratedEngine.Evaluate(string.Format(@"incomedEnvironment{0} <- unserialize(serializedList{0});
                        //                                             sapply(names(incomedEnvironment{0}), 
                        //                                             function(elem{0}) {{ assign(elem{0}, incomedEnvironment{0}[[elem{0}]], envir = .GlobalEnv); }})",
                        //                                         InternalRandomIdentifier));


                        DecoratedEngine.SetRenewedEnvironment(resultEnvironment, InternalRandomIdentifier);

                        var environmentMessage = new EnvironmentWideMessage(new[] { parsedMessage.Item2 });
                        ((TCPServer)this.Сonnector).MulticastSendMessage(environmentMessage, new long[] { freeElem.Key });
                    }
                    else
                    {
                        ((TCPServer)this.Сonnector).MulticastSendMessage(requestMessage);
                    }
                }
            }

            if (attributes.First(elem => elem.Key.Equals(typeof(InternalExecutionAttribute))).Value)
            {
                var internalExecutionResult = execute(objectToInvoke, parameters.ToArray());
                resultCollection = new SymbolicExpression[] { internalExecutionResult as SymbolicExpression };

                if (attributes.First(elem => elem.Key.Equals(typeof(MulticastExecutionAttribute))).Value)
                {
                    /*
                     var packingScript = string.Format(@"changedVariablesName{0} <- ls(globalenv());
                                                        changedVariableValues{0} <- sapply(changedVariablesName{0}, function(elem{0}) get(elem{0}));
                                                        changedVariableList{0} <- setNames(as.list(changedVariableValues{0}), changedVariablesName{0});",
                                    InternalRandomIdentifier);

                    var serializedEnvironment = RHelper.CreateRSerializedAnswer(DecoratedEngine, DecoratedEngine.Evaluate(packingScript), InternalRandomIdentifier);
                    */

                    var environment = DecoratedEngine.GetEnvironmentList(InternalRandomIdentifier, ".GlobalEnv", RHelper.RSystemWidedInternals);
                    var serializedEnvironment = DecoratedEngine.SerializeRObject(environment, InternalRandomIdentifier);

                    var environmentMessage = new EnvironmentWideMessage(new[] { serializedEnvironment });
                    ((TCPServer)this.Сonnector).MulticastSendMessage(environmentMessage, new long[] { freeElem.Key });
                }
            }

            DecoratedEngine.PrintEnvironmentNames();

            clientID[freeElem.Key] = true;
            semaphores.Release(1);

            return resultCollection;


            #region old environment swap variant
            /*




            string getEnvironmentScript = "globalenv()";
            var getEnvironmentMessage = new RequestMessage("Evaluate", new object[1] { getEnvironmentScript });

            Сonnector.SendMessage(getEnvironmentMessage, clientParams);
            var renewedGlobalEnv = Сonnector.ReceiveMessage(clientParams);

            var setNewEnvironmentMessage = new RequestMessage("SetSymbol", new object [] {"newConcatenationEnvironment", renewedGlobalEnv}, false);
            ((TCPServer)Сonnector).MulticastSendMessage(setNewEnvironmentMessage, new long[] { freeElem.Key });

            string concatEnvironments = @"dataList <- ls(newConcatenationEnvironment);
                                          for(v in dataList) 
                                          {
	                                            assign(v, newConcatenationEnvironment[[v]], envir = .GlobalEnv)
                                          }";

            var concatenateEnvironmentsMessage = new RequestMessage("Evaluate", new object[1] { concatEnvironments }, false);
            ((TCPServer)Сonnector).MulticastSendMessage(concatenateEnvironmentsMessage, new long[] { freeElem.Key });

            clientID[freeElem.Key] = true;
            semaphores.Release(1);

            return mainResult.ElementAt(0);
            
            */
            #endregion
        }
    }
}
