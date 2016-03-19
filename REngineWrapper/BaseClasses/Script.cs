using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace RManaged.BaseTypes
{
    public class Script
    {
        public string ScriptBody { get; protected set; }
        public bool IsValid { get; protected set; }
        public bool IsInitialized { get; protected set; }
        public List<Tuple<string, bool>> InternalFunctions { get; protected set; }

        public Script(string scriptName, [Optional] ICollection<Tuple<string, bool>> internalFunctions, [Optional] object[] identifiers)
        {
            IsValid = false;
            IsInitialized = false;
            InternalFunctions = null;

            using (TextReader reader = File.OpenText(scriptName))
            {
                string ScriptBody = reader.ReadToEnd();
                IsValid = true;

                InternalFunctions = new List<Tuple<string, bool>>();

                if (internalFunctions != null)
                    InternalFunctions.AddRange(internalFunctions);
            }

            if (identifiers != null)
            {
                ScriptBody = string.Format(ScriptBody, identifiers);
            }
        }
    }
}
