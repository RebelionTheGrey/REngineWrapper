using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RManaged.BaseTypes
{
    public class Script
    {
        public string ScriptBody { get; protected set; }
        public bool IsValid { get; protected set; }
        public bool IsInitialized { get; protected set; }
        public List<Tuple<string, bool>> InternalFunctions { get; protected set; }

        public Script(string scriptName, IEnumerable<Tuple<string, bool>> internalFunctions)
        {
            IsValid = false;
            IsInitialized = false;

            using (TextReader reader = File.OpenText(scriptName))
            {
                string ScriptBody = reader.ReadToEnd();
                IsValid = true;

                InternalFunctions = new List<Tuple<string, bool>>();

                if (internalFunctions != null)
                    InternalFunctions.AddRange(internalFunctions);
            }
        }
    }
}
