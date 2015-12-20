using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using RDotNet;
using RDotNet.NativeLibrary;

using BaseTypes;

namespace Messages
{
    [Serializable]
    public class BaseMessage
    {
        public virtual ulong ID { get; set; }
    }

    [Serializable]
    public class MethodCallMessage : BaseMessage, IMethodCall, IDataCollection<object>
    {
        protected string methodName;
        protected readonly ICollection<object> parameters;

        public ICollection<Type> MethodAttributes { get; protected set; }

        public string GetMethod()
        {
            return methodName;
        }
        public ICollection<object> GetData()
        {
            return parameters;
        }
        public MethodCallMessage(string methodName, ICollection<object> parameters, ICollection<Type> methodAttributes)
        {
            this.methodName = methodName;
            this.parameters = parameters;
            MethodAttributes = methodAttributes;
        }
    }    

    [Serializable]
    public class EnvironmentWideMessage : BaseMessage, IEnvironment, IDataCollection<byte []>
    {
        protected ICollection<byte []> data;
        protected string environmentName;

        public ICollection<byte []> GetData()
        {
            return data;
        }

        public string GetEnvironmentName()
        {
            return environmentName;
        }

        public EnvironmentWideMessage(ICollection<byte []> data)
        {
            this.data = data;
        }
    }    

    [Serializable]
    public class AnswerMessage : BaseMessage, IDataDictionary<string, ICollection<byte []>>
    {
        public bool IsValidAnswer { get; protected set; }
        protected IDictionary<string, ICollection<byte[]>> SerializedData { get; set; }

        public AnswerMessage(IDictionary<string, ICollection<byte[]>> data, bool isValidAnswer)
        {
            this.SerializedData = data;
            IsValidAnswer = isValidAnswer;
        }

        public IDictionary<string, ICollection<byte[]>> GetData()
        {
            return SerializedData; 
        }
        public ICollection<byte[]> GetData(string key)
        {
            return SerializedData.FirstOrDefault(elem => elem.Key.Equals(key)).Value;
        }
    }

    [Serializable]
    public class TransferMessageWrapper : EventArgs
    {
        public virtual BaseMessage Message { get; set; }
        public TransferMessageWrapper(BaseMessage message)
        {
            Message = message;
        }
    }
}

