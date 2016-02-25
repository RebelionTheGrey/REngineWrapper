using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RManaged.Communications;

using MathNet.Numerics.Random;



namespace RManaged.BaseTypes
{
    public class BaseLayer
    {
        public string ConfigName { get; protected set; }
        protected long InternalRandomIdentifier { get; set; }
        protected virtual CommunicationProtocol Сonnector { get; set; }
        protected BaseLayer()
        {
            InternalRandomIdentifier = (new Random()).NextInt64();
        }
    }
}
