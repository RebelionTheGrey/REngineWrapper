using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using RDotNet;
using RDotNet.NativeLibrary;

using RManaged.BaseTypes;
using MathNet.Numerics.Random;

namespace RManaged.Communications
{

}



namespace RManaged.Communications
{
    [Serializable]
    public abstract class BaseMessage
    {
        private static RandomSource randomSource;
        static BaseMessage()
        {
            randomSource = new Mcg59();
        }

        public long ID { get; protected set; }

        public BaseMessage() : this(RandomExtensions.NextInt64(randomSource)) { }
        public BaseMessage(long id)
        {
            ID = id;
        }
    }

    [Serializable]
    public class MethodCallMessage : BaseMessage
    {
        public string MethodName { get; protected set; }
        public ICollection<object> Parameters { get; protected set; }
        public ICollection<Type> MethodAttributes { get; protected set; }

        public MethodCallMessage(string methodName, ICollection<object> parameters, ICollection<Type> methodAttributes)
        {
            MethodName = methodName;
            Parameters = parameters;
            MethodAttributes = methodAttributes;
        }
    }    

    [Serializable]
    public class EnvironmentWideMessage : BaseMessage
    { 
        public ICollection<byte []> Data { get; protected set; }
        public string EnvironmentName { get; protected set; }

        public EnvironmentWideMessage(string environmentName, ICollection<byte []> data)
        {
            Data = data;
            EnvironmentName = environmentName;
        }
    }    

    [Serializable]
    public class AnswerMessage : BaseMessage
    {
        public bool IsValidAnswer { get; protected set; }

        public ICollection<byte []> SerializedResult { get; protected set; }
        public ICollection<byte []> SerializedEnvironment { get; protected set; }

        public AnswerMessage(ICollection<byte []> serializedResult, ICollection<byte[]> serializedEnvironment, bool isValidAnswer)
        {
            SerializedResult = serializedResult;
            SerializedEnvironment = serializedEnvironment;

            IsValidAnswer = isValidAnswer;
        }
    }

    [Serializable]
    public class EmptyMessage : BaseMessage
    {

    }

    [Serializable]
    public class TransferMessageWrapper : EventArgs
    {
        public virtual BaseMessage Message { get; protected set; }
        public TransferMessageWrapper(BaseMessage message)
        {
            Message = message;
        }
    }
}

