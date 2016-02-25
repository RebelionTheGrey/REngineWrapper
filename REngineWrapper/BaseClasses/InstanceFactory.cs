using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using RManaged.Communications;



namespace RManaged.BaseTypes
{
    public abstract class AbstractStorehouse<T, V>
    {
        protected IDictionary<T, IEnumerable<V>> storehouse;

        public abstract T Find(IEnumerable<V> listOfFormals);
        public abstract void Add(T obj, IEnumerable<V> listOfFormals);
        public abstract void Remove(T obj);
        public abstract void Renew(T obj, IEnumerable<V> listOfFormals);
    }

}

namespace RManaged.Core
{
    using RManaged.BaseTypes;

    public class HandlerStoreHouse : AbstractStorehouse<InternalFunctionPresentation, Type>
    {
        protected InternalFunctionPresentation emptyDelegate;

        protected void Initialize()
        {
            storehouse = new Dictionary<InternalFunctionPresentation, IEnumerable<Type>>();
            emptyDelegate = delegate { };
        }
        public HandlerStoreHouse()
        {
            Initialize();
        }

        public override InternalFunctionPresentation Find(IEnumerable<Type> listOfFormals)
        {
             var conditionalSet = storehouse.Where((storeElem) =>
             {
                var intersect = listOfFormals.Intersect(storeElem.Value);
                return intersect.Count() == listOfFormals.Count();
             });

            var result = conditionalSet.Select(e => e.Key);

            return result.Count() > 0 ? result.ElementAt(0) : emptyDelegate;            
        }
        public override void Add(InternalFunctionPresentation obj, IEnumerable<Type> listOfFormals)
        {
            if (!storehouse.ContainsKey(obj))
                storehouse.Add(obj, listOfFormals);
        }
        public override void Remove(InternalFunctionPresentation obj)
        {
            if (storehouse.ContainsKey(obj))
                storehouse.Remove(obj);
        }
        public override void Renew(InternalFunctionPresentation obj, IEnumerable<Type> listOfFormals)
        {
            if (storehouse.ContainsKey(obj))
                storehouse[obj] = listOfFormals;
        }

        public void EntryPoint(object sender, EventArgs message)
        {
            var incomingMessage = ((TransferMessageWrapper)message).Message;

            Type[] messageMethods = incomingMessage.GetType().GetInterfaces();

            var ExecuteMethod = Find(messageMethods);
            ExecuteMethod(incomingMessage);
        }
    }
}
