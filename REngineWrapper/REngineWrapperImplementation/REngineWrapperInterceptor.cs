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

            var methodAttributes = typeof(IREngine).GetMethod(methodName, parameters.Select(elem => { return elem.GetType(); }).ToArray()).GetCustomAttributes(false).Cast<Type>().ToList();


            ICollection<SymbolicExpression> resultCollection = null;            

            if (methodAttributes.Contains(typeof(ExternalExecutionAttribute)))
            {
                var requestMessage = new MethodCallMessage(methodName, parameters, methodAttributes);

                if (!methodAttributes.Contains(typeof(MulticastExecutionAttribute)))
                {
                    var clientParams = new Dictionary<string, object>();
                    clientParams.Add("clientID", freeElem.Key);

                    Сonnector.SendMessage(requestMessage, clientParams);

                    if (methodAttributes.Contains(typeof(AnswerableAttribute)))
                    {
                        var incomedMessage = Сonnector.ReceiveMessage(clientParams);
                        var parsedMessage = ParseAnswerMessage(incomedMessage);
                        resultCollection = parsedMessage.Item1;

                        var resultEnvironment = parsedMessage.Item2;

                        if (methodAttributes.Contains(typeof(AnswerableAttribute)))
                        {

                            DecoratedEngine.SetRenewedEnvironment(resultEnvironment, InternalRandomIdentifier);

                            var environmentMessage = new EnvironmentWideMessage(RHelper.GlobalEnvironmentName, new[] { parsedMessage.Item2 });
                            ((TCPServer)this.Сonnector).MulticastSendMessage(environmentMessage, new long[] { freeElem.Key });
                        }
                    }
                    else
                    {
                        ((TCPServer)this.Сonnector).MulticastSendMessage(requestMessage);
                    }
                }
            }

            if (methodAttributes.Contains(typeof(InternalExecutionAttribute)))
            {
                var internalExecutionResult = execute(objectToInvoke, parameters.ToArray());
                resultCollection = new SymbolicExpression[] { internalExecutionResult as SymbolicExpression };

                if (methodAttributes.Contains(typeof(EnvironmentSwapAttribute)))
                {
                    var environment = DecoratedEngine.GetEnvironmentList(InternalRandomIdentifier, RHelper.GlobalEnvironmentName, RHelper.RSystemWidedInternals);
                    var serializedEnvironment = DecoratedEngine.SerializeRObject(environment, InternalRandomIdentifier);

                    var environmentMessage = new EnvironmentWideMessage(RHelper.GlobalEnvironmentName,  new[] { serializedEnvironment });
                    ((TCPServer)this.Сonnector).MulticastSendMessage(environmentMessage, new long[] { freeElem.Key });
                }
            }

            clientID[freeElem.Key] = true;
            semaphores.Release(1);

            return resultCollection;
        }
    }
}
