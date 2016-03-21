using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RDotNet;
using RDotNet.NativeLibrary;

using RManaged.Communications;

namespace RManaged.BaseTypes
{
    public delegate void MessageReceiveHandler(object sender, EventArgs message);
    public delegate void InternalFunctionPresentation(BaseMessage message);

    public static class Empty<T>
    {
        public static Task<T> Task { get { return _task; } }

        private static readonly Task<T> _task = System.Threading.Tasks.Task.FromResult(default(T));
    }

    public class ExternalExecutionAttribute : Attribute { } //need to execute on same slave node
    public class InternalExecutionAttribute : Attribute { } //need to execute just on main node
    public class MulticastExecutionAttribute : Attribute { } //need to execute on all slave nodes
    public class EnvironmentSwapAttribute : Attribute { } //need to send renewed enviroment on all slave nodes
    public class AnswerableAttribute : Attribute { } //need to send execution result back to the main node
}
