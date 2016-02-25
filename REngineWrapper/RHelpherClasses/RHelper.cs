using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RDotNet;
using RDotNet.NativeLibrary;


namespace RManaged.Utilities
{
    public static class RHelper
    {
        public readonly static List<string> RSystemWidedInternals = new List<string>()
        {
            "dataToSerialize", "serializedData", "exceptNames", "changedVariablesName", "changedVariableValues", "changedVariableList", "elem",
            "incomedData", "serializedEnvironmentData", "objectToDeserialize", "deserializedRObject", "internalCluster", "maxThreads"
        };

        public readonly static List<string> RSystemInternals = new List<string>()
        {

        };

        public static byte[] SerializeRObject(this REngine engine, SymbolicExpression dataToRSerialize, long identifier)
        {
            var rawDataName = string.Format("dataToSerialize{0}", identifier);

            engine.SetSymbol(rawDataName, dataToRSerialize);
            var byteArray = engine.Evaluate(string.Format("serializedData{0} <- serialize(dataToSerialize{0}, NULL);", identifier))
                                           .AsRaw().ToArray();

            return byteArray;
        }
        public static SymbolicExpression GetEnvironmentList(this REngine engine, long identifier, string environment = ".GlobalEnv", params ICollection<string> [] except)
        {
            string variableNamesSequence = "fakeVariable"; //stupid method, but...

            if (except != null)
            {
                foreach (var elem in except)
                {
                    var widedNames = elem.Select(s => { return string.Format(s+@"{0}", identifier); });
                    variableNamesSequence = string.Format(string.Join(@""", """, widedNames), identifier);
                }
            }

            variableNamesSequence = @"""" + variableNamesSequence + @"""";

            Console.WriteLine("Total except variables string is " + variableNamesSequence);

            engine.Evaluate(string.Format(@"exceptNames{0} <- c({1})", identifier, variableNamesSequence));

            var packingScript = string.Format(@"changedVariablesName{0} <- ls({1});
                                                changedVariablesName{0} <- changedVariablesName{0}[!(changedVariablesName{0} %in% exceptNames{0})];

                                                print(""environment names:"");
                                                print(changedVariablesName{0});
                                                print(""exceptNames:"");
                                                print(exceptNames{0});




                                                changedVariableValues{0} <- sapply(changedVariablesName{0}, function(elem{0}) get(elem{0}));
                                                changedVariableList{0} <- setNames(as.list(changedVariableValues{0}), changedVariablesName{0});",
                                              identifier, environment);

            return engine.Evaluate(packingScript);
        }
        public static void SetRenewedEnvironment(this REngine engine, byte [] serializedEnvironment, long identifier, string environmentName = ".GlobalEnv")
        {
            var rawRVector = engine.CreateRawVector(serializedEnvironment);
            engine.SetSymbol(string.Format("serializedEnvironmentData{0}", identifier), rawRVector);

            engine.Evaluate(string.Format(@"incomedData{0} <- unserialize(serializedEnvironmentData{0});
                                            sapply(names(incomedData{0}), 
                                            function(elem{0}) {{ assign(elem{0}, incomedData{0}[[elem{0}]], envir = {1}); }})",
                                          identifier, environmentName));
        }
        public static SymbolicExpression DeserializeRObject(this REngine engine, byte [] serializedObject, long identifier)
        {
            engine.SetSymbol(string.Format("objectToDeserialize{0}", identifier), engine.CreateRawVector(serializedObject));
            engine.Evaluate(string.Format("deserializedRObject{0} <- unserialize(objectToDeserialize{0});", identifier));
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
