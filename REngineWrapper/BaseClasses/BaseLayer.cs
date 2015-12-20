using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Messages;
using Communications;

using MathNet.Numerics.Random;



namespace RManaged
{
    public class BaseLayer
    {
        public string LogName { get; protected set; }
        protected long InternalRandomIdentifier { get; set; }
        protected virtual CommunicationProtocol Сonnector { get; set; }
        protected BaseLayer()
        {
            InternalRandomIdentifier = (new Random()).NextInt64();
        }
    }
}
