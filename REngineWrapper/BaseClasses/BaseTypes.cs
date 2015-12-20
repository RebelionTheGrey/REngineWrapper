using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RDotNet;
using RDotNet.NativeLibrary;

using Messages;

namespace BaseTypes
{
    //Initialization interface.
    public interface IMethodCall
    {
        string GetMethod();
    }
    public interface IDataCollection<T>
    {
        ICollection<T> GetData();
    }    
    public interface IDataDictionary<T, V>
    {
        IDictionary<T, V> GetData();
        V GetData(T key);
    }
    public interface IEnvironment 
    {
        string GetEnvironmentName();
    }


    public delegate void MessageReceiveHandler(object sender, EventArgs message);
    public delegate void InternalFunctionPresentation(BaseMessage message);

    public static class Empty<T>
    {
        public static Task<T> Task { get { return _task; } }

        private static readonly Task<T> _task = System.Threading.Tasks.Task.FromResult(default(T));
    }

    public class ExternalExecutionAttribute : Attribute { }
    public class InternalExecutionAttribute : Attribute { }
    public class MulticastExecutionAttribute : Attribute { }
    public class EnvironmentSwapAttribute : Attribute { }
    public class AnswerableAttribute : Attribute { }
}
