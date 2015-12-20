﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RDotNet;
using RDotNet.NativeLibrary;
using RDotNet.Devices;
using RDotNet.Utilities;

using System.IO;


namespace BaseTypes
{
    public interface IREngine
    {
        [InternalExecution]
        bool AutoPrint { get; set; }

        [InternalExecution]
        REnvironment BaseNamespace { get; }

        [InternalExecution]
        bool Disposed { get; }

        [InternalExecution]
        string DllVersion { get; }

        [InternalExecution]
        REnvironment EmptyEnvironment { get; }

        [InternalExecution]
        bool EnableLock { get; set; }

        //static string EngineName { get; }
        [InternalExecution]
        REnvironment GlobalEnvironment { get; }

        [InternalExecution]
        string ID { get; }

        [InternalExecution]
        bool IsRunning { get; }

        [InternalExecution]
        SymbolicExpression NaString { get; }

        [InternalExecution]
        IntPtr NaStringPointer { get; }

        [InternalExecution]
        SymbolicExpression NilValue { get; }

        [InternalExecution]
        SymbolicExpression UnboundValue { get; }

        [InternalExecution]
        event EventHandler Disposing;
        //static string[] BuildRArgv(StartupParameter parameter);

        [ExternalExecution, MulticastExecution, InternalExecution]
        void ClearGlobalEnvironment(bool garbageCollectR = true, bool garbageCollectDotNet = true, bool removeHiddenRVars = false, bool detachPackages = false, string[] toDetach = null);
        
        [ExternalExecution, MulticastExecution, InternalExecution]
        SymbolicExpression CreateFromNativeSexp(IntPtr sexp);

        [InternalExecution, Answerable]
        IEnumerable<SymbolicExpression> Defer(Stream stream);
        //static void DoDotNetGarbageCollection();
        
        [ExternalExecution, Answerable, EnvironmentSwap]
        SymbolicExpression Evaluate(Stream stream);

        [ExternalExecution, Answerable, EnvironmentSwap]
        SymbolicExpression Evaluate(string statement);

        [ExternalExecution, InternalExecution]
        void ForceGarbageCollection();
        //static REngine GetInstance(string dll = null, bool initialize = true, StartupParameter parameter = null, ICharacterDevice device = null);

        [InternalExecution]
        SymbolicExpression GetPredefinedSymbol(string name);

        [InternalExecution]
        SymbolicExpression GetSymbol(string name);

        [InternalExecution]
        SymbolicExpression GetSymbol(string name, REnvironment environment);
        //static int[] IndexOfAll(string sourceString, string matchString);

        //[InternalExecution]
        //void Initialize(StartupParameter parameter = null, ICharacterDevice device = null, bool setupMainLoop = true);

        [InternalExecution, MulticastExecution]
        void SetCommandLineArguments(string[] args);
        //static void SetEnvironmentVariables(string rPath = null, string rHome = null);

        [InternalExecution, MulticastExecution]
        void SetSymbol(string name, SymbolicExpression expression);

        [InternalExecution, MulticastExecution]
        void SetSymbol(string name, SymbolicExpression expression, REnvironment environment);

        [InternalExecution]
        REngine DecoratedEngine { get; }
    }
}
