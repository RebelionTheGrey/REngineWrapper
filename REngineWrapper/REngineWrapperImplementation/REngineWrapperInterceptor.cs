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
using RManaged.Extensions;

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
                             GetCustomAttributes(false); 

            //Dummy code, need to make auxiliary static class of all types.
            var typeCollection = new List<Type>() { typeof(InternalExecutionAttribute), typeof(ExternalExecutionAttribute),
                                                    typeof(AnswerableAttribute), typeof(MulticastExecutionAttribute)};

            IDictionary<Type, bool> attributes = new Dictionary<Type, bool>();

            typeCollection.ForEach(attr =>
            {
                attributes.Add(attr, methodAttributes.Any(elem => elem.GetType().Equals(attr)));
            });

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
        }
    }
}
