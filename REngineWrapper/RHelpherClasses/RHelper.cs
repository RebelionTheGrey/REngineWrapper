using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RDotNet;
using RDotNet.NativeLibrary;

using RManaged.BaseTypes;

using System.Runtime.InteropServices;

namespace RManaged.Utilities
{
    public static class RHelper
    {
        public static Script GetEnvironmentListScript { get; private set; }
        public static Script SetRenewedEnvironmentListScript { get; private set; }
        public static Script SerializeRObjectScript { get; private set; }
        public static Script DeserializeRObjectScript { get; private set; }
        public static string GlobalEnvironmentName { get { return ".GlobalEnv"; } }

        public readonly static List<string> RSystemWidedInternals;
        public readonly static List<string> RSystemInternals;

        static RHelper()
        {
            RSystemInternals = new List<string>()
            {
                "dataToSerialize", "serializedData", "exceptNames", "changedVariablesName", "changedVariableValues", "changedVariableList", "elem",
                "incomedData", "serializedEnvironmentData", "objectToDeserialize", "deserializedRObject", "internalCluster", "maxThreads", "totalInternalVariablesList"
            };

            RSystemWidedInternals = RSystemInternals.Select(elem => { return (elem + "{0}"); }).ToList();

            GetEnvironmentListScript = new Script("GetEnvironmentListScript.Rwided", null);
            SetRenewedEnvironmentListScript = new Script("SetRenewedEnvironmentScript.Rwided", null);
            SerializeRObjectScript = new Script("SerializeRObject.Rwided", null);
            DeserializeRObjectScript = new Script("DeserializeRObject.Rwided", null);
        }

        public static string GetWidedScript(string baseScript, params object [] elems)
        {
            return string.Format(baseScript, elems);
        }

        public static byte[] SerializeRObject(this REngine engine, SymbolicExpression dataToRSerialize, long identifier)
        {
            var rawDataName = string.Format("dataToSerialize{0}", identifier);
            engine.SetSymbol(rawDataName, dataToRSerialize);

            var widedScript = GetWidedScript(GetEnvironmentListScript.ScriptBody, identifier);
            var byteArray = engine.Evaluate(widedScript).AsRaw().ToArray();

            return byteArray;
        }
        public static SymbolicExpression GetEnvironmentList(this REngine engine, long identifier, [Optional] string environmentName, [Optional] ICollection<string> except)
        {
            var replacedInternals = RSystemWidedInternals.Select(elem => { return string.Format(elem, identifier); });
            var RInternalsVector = engine.CreateCharacterVector(replacedInternals);

            engine.SetSymbol(string.Format("exceptNames{0}", identifier), RInternalsVector);

            var dataEnvironment = environmentName != default(string) ? GlobalEnvironmentName : environmentName;
            var widedScript = GetWidedScript(GetEnvironmentListScript.ScriptBody, identifier, dataEnvironment);

            return engine.Evaluate(widedScript);
        }
        public static void SetRenewedEnvironment(this REngine engine, byte [] serializedEnvironment, long identifier, [Optional] string environmentName)
        {
            var rawRVector = engine.CreateRawVector(serializedEnvironment);
            engine.SetSymbol(string.Format("serializedEnvironmentData{0}", identifier), rawRVector);

            var dataEnvironment = environmentName != default(string) ? GlobalEnvironmentName : environmentName;
            var widedScript = GetWidedScript(SetRenewedEnvironmentListScript.ScriptBody, identifier, dataEnvironment);

            engine.Evaluate(widedScript);
        }
        public static SymbolicExpression DeserializeRObject(this REngine engine, byte [] serializedObject, long identifier)
        {
            engine.SetSymbol(string.Format("objectToDeserialize{0}", identifier), engine.CreateRawVector(serializedObject));

            var widedScript = GetWidedScript(SetRenewedEnvironmentListScript.ScriptBody, identifier);
            return engine.GetSymbol(string.Format("deserializedRObject{0}", identifier));
        }
        public static void PrintEnvironmentNames(this REngine engine, string environment = ".GlobalEnv")
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("Environment:");
            engine.Evaluate(string.Format(@"ls({0})", environment));
            Console.WriteLine("--------------------------------------");
        }

    }
}
