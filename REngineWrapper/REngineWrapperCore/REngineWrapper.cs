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


    public static class REngineExtensionWrapper
    {
        public static CharacterVector CreateCharacter(this IREngine engine, string value)
        {
            return engine.DecoratedEngine.CreateCharacter(value);
        }
        public static CharacterMatrix CreateCharacterMatrix(this IREngine engine, string[,] matrix)
        {
            return engine.DecoratedEngine.CreateCharacterMatrix(matrix);
        }
        public static CharacterMatrix CreateCharacterMatrix(this IREngine engine, int rowCount, int columnCount)
        {
            return engine.DecoratedEngine.CreateCharacterMatrix(rowCount, columnCount);
        }
        public static CharacterVector CreateCharacterVector(this IREngine engine, IEnumerable<string> vector)
        {
            return engine.DecoratedEngine.CreateCharacterVector(vector);
        }
        public static CharacterVector CreateCharacterVector(this IREngine engine, int length)
        {
            return engine.DecoratedEngine.CreateCharacterVector(length);
        }
        public static ComplexVector CreateComplex(this IREngine engine, Complex value)
        {
            return engine.DecoratedEngine.CreateComplex(value);
        }
        public static ComplexMatrix CreateComplexMatrix(this IREngine engine, Complex[,] matrix)
        {
            return engine.DecoratedEngine.CreateComplexMatrix(matrix);
        }
        public static ComplexMatrix CreateComplexMatrix(this IREngine engine, int rowCount, int columnCount)
        {
            return engine.DecoratedEngine.CreateComplexMatrix(rowCount, columnCount);
        }
        public static ComplexVector CreateComplexVector(this IREngine engine, IEnumerable<Complex> vector)
        {
            return engine.DecoratedEngine.CreateComplexVector(vector);
        }
        public static ComplexVector CreateComplexVector(this IREngine engine, int length)
        {
            return engine.DecoratedEngine.CreateComplexVector(length);
        }
        public static DataFrame CreateDataFrame(this IREngine engine, IEnumerable[] columns, string[] columnNames = null, string[] rowNames = null, bool checkRows = false, bool checkNames = true, bool stringsAsFactors = true)
        {
            return engine.DecoratedEngine.CreateDataFrame(columns, columnNames, rowNames, checkRows, checkNames, stringsAsFactors);
        }
        public static REnvironment CreateEnvironment(this IREngine engine, REnvironment parent)
        {
            return engine.DecoratedEngine.CreateEnvironment(parent);
        }
        public static IntegerVector CreateInteger(this IREngine engine, int value)
        {
            return engine.DecoratedEngine.CreateInteger(value);
        }
        public static IntegerMatrix CreateIntegerMatrix(this IREngine engine, int[,] matrix)
        {
            return engine.DecoratedEngine.CreateIntegerMatrix(matrix);
        }
        public static IntegerMatrix CreateIntegerMatrix(this IREngine engine, int rowCount, int columnCount)
        {
            return engine.DecoratedEngine.CreateIntegerMatrix(rowCount, columnCount);
        }
        public static IntegerVector CreateIntegerVector(this IREngine engine, IEnumerable<int> vector)
        {
            return engine.DecoratedEngine.CreateIntegerVector(vector);
        }
        public static IntegerVector CreateIntegerVector(this IREngine engine, int length)
        {
            return engine.DecoratedEngine.CreateIntegerVector(length);
        }
        public static REnvironment CreateIsolatedEnvironment(this IREngine engine)
        {
            return engine.DecoratedEngine.CreateIsolatedEnvironment();
        }
        public static LogicalVector CreateLogical(this IREngine engine, bool value)
        {
            return engine.DecoratedEngine.CreateLogical(value);
        }
        public static LogicalMatrix CreateLogicalMatrix(this IREngine engine, bool[,] matrix)
        {
            return engine.DecoratedEngine.CreateLogicalMatrix(matrix);
        }
        public static LogicalMatrix CreateLogicalMatrix(this IREngine engine, int rowCount, int columnCount)
        {
            return engine.DecoratedEngine.CreateLogicalMatrix(rowCount, columnCount);
        }
        public static LogicalVector CreateLogicalVector(this IREngine engine, IEnumerable<bool> vector)
        {
            return engine.DecoratedEngine.CreateLogicalVector(vector);
        }
        public static LogicalVector CreateLogicalVector(this IREngine engine, int length)
        {
            return engine.DecoratedEngine.CreateLogicalVector(length);
        }
        public static NumericVector CreateNumeric(this IREngine engine, double value)
        {
            return engine.DecoratedEngine.CreateNumeric(value);
        }
        public static NumericMatrix CreateNumericMatrix(this IREngine engine, double[,] matrix)
        {
            return engine.DecoratedEngine.CreateNumericMatrix(matrix);
        }
        public static NumericMatrix CreateNumericMatrix(this IREngine engine, int rowCount, int columnCount)
        {
            return engine.DecoratedEngine.CreateNumericMatrix(rowCount, columnCount);
        }
        public static NumericVector CreateNumericVector(this IREngine engine, IEnumerable<double> vector)
        {
            return engine.DecoratedEngine.CreateNumericVector(vector);
        }
        public static NumericVector CreateNumericVector(this IREngine engine, int length)
        {
            return engine.DecoratedEngine.CreateNumericVector(length);
        }
        public static RawVector CreateRaw(this IREngine engine, byte value)
        {
            return engine.DecoratedEngine.CreateRaw(value);
        }
        public static RawMatrix CreateRawMatrix(this IREngine engine, byte[,] matrix)
        {
            return engine.DecoratedEngine.CreateRawMatrix(matrix);
        }
        public static RawMatrix CreateRawMatrix(this IREngine engine, int rowCount, int columnCount)
        {
            return engine.DecoratedEngine.CreateRawMatrix(rowCount, columnCount);
        }
        public static RawVector CreateRawVector(this IREngine engine, IEnumerable<byte> vector)
        {
            return engine.DecoratedEngine.CreateRawVector(vector);
        }
        public static RawVector CreateRawVector(this IREngine engine, int length)
        {
            return engine.DecoratedEngine.CreateRawVector(length);
        }
    }

}
