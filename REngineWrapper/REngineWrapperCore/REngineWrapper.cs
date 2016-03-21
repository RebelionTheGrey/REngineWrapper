using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using RDotNet;
using RDotNet.NativeLibrary;
using RDotNet.Devices;
using RDotNet.Dynamic;
using RDotNet.Internals;

using RManaged.BaseTypes;


namespace RManaged.Core
{
    public sealed partial class REngineWrapper : BaseLayer, IREngine
    {
        public bool AutoPrint
        {
            get
            {
                return DecoratedEngine.AutoPrint;
            }
            set
            {
                DecoratedEngine.AutoPrint = value;
            }
        }
        public REnvironment BaseNamespace
        {
            get { return DecoratedEngine.BaseNamespace; }
        }
        public bool Disposed
        {
            get { return DecoratedEngine.Disposed; }
        }
        public string DllVersion
        {
            get { return DecoratedEngine.DllVersion; }
        }
        public REnvironment EmptyEnvironment
        {
            get { return DecoratedEngine.EmptyEnvironment; }
        }
        public bool EnableLock
        {
            get
            {
                return DecoratedEngine.EnableLock;
            }
            set
            {
                DecoratedEngine.EnableLock = value;
            }
        }
        public REnvironment GlobalEnvironment
        {
            get { return DecoratedEngine.GlobalEnvironment; }
        }
        public string ID
        {
            get { return DecoratedEngine.ID; }
        }
        public bool IsRunning
        {
            get { return DecoratedEngine.IsRunning; }
        }
        public SymbolicExpression NaString
        {
            get { return DecoratedEngine.NaString; }
        }
        public IntPtr NaStringPointer
        {
            get { return DecoratedEngine.NaStringPointer; }
        }
        public SymbolicExpression NilValue
        {
            get { return DecoratedEngine.NilValue; }
        }
        public SymbolicExpression UnboundValue
        {
            get { return DecoratedEngine.UnboundValue; }
        }
        public event EventHandler Disposing;
        public void ClearGlobalEnvironment(bool garbageCollectR = true, bool garbageCollectDotNet = true, bool removeHiddenRVars = false, bool detachPackages = false, string[] toDetach = null)
        {
            DecoratedEngine.ClearGlobalEnvironment(garbageCollectR, garbageCollectDotNet, removeHiddenRVars, detachPackages, toDetach);
        }
        public SymbolicExpression CreateFromNativeSexp(IntPtr sexp)
        {
            return DecoratedEngine.CreateFromNativeSexp(sexp);
        }
        public IEnumerable<SymbolicExpression> Defer(System.IO.Stream stream)
        {
            return DecoratedEngine.Defer(stream);
        }
        public SymbolicExpression Evaluate(System.IO.Stream stream)
        {
            return DecoratedEngine.Evaluate(stream);
        }
        public SymbolicExpression Evaluate(string statement)
        {
            return DecoratedEngine.Evaluate(statement);
        }
        public void MulticastEvaluate(string statement) { DecoratedEngine.Evaluate(statement); }
        public void ForceGarbageCollection()
        {
            DecoratedEngine.ForceGarbageCollection();
        }
        public SymbolicExpression GetPredefinedSymbol(string name)
        {
            return DecoratedEngine.GetPredefinedSymbol(name);
        }
        public SymbolicExpression GetSymbol(string name)
        {
            return DecoratedEngine.GetSymbol(name);
        }
        public SymbolicExpression GetSymbol(string name, REnvironment environment)
        {
            return DecoratedEngine.GetSymbol(name, environment);
        }
        public void Initialize(StartupParameter parameter = null, ICharacterDevice device = null, bool setupMainLoop = true)
        {
            DecoratedEngine.Initialize(parameter, device, setupMainLoop);
        }
        public void SetCommandLineArguments(string[] args)
        {
            DecoratedEngine.SetCommandLineArguments(args);
        }
        public void SetSymbol(string name, SymbolicExpression expression)
        {
            DecoratedEngine.SetSymbol(name, expression);
        }
        public void SetSymbol(string name, SymbolicExpression expression, REnvironment environment)
        {
            DecoratedEngine.SetSymbol(name, expression, environment);
        }
        public static string EngineName { get { return REngine.EngineName; } }
        public static string[] BuildRArgv(StartupParameter parameter) { return REngine.BuildRArgv(parameter); }
        public static void DoDotNetGarbageCollection() { REngine.DoDotNetGarbageCollection(); }
        public static REngineWrapper GetInstance(string dll = null, bool initialize = true, StartupParameter parameter = null, ICharacterDevice device = null)
        {
            throw new NotImplementedException();
        }
        public static int[] IndexOfAll(string sourceString, string matchString) { return REngine.IndexOfAll(sourceString, matchString); }
        public static void SetEnvironmentVariables(string rPath = null, string rHome = null) { REngine.SetEnvironmentVariables(rPath, rHome); }
    }
}
