using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RDotNet;
using RDotNet.NativeLibrary;
using RDotNet.Devices;
using RDotNet.Dynamic;
using RDotNet.Internals;

using RManaged.BaseTypes;

namespace RManaged.Extensions
{
    public static class SymbolicExpressionExtensionWrapper
    {
        public static CharacterVector AsCharacter(this SymbolicExpression expression)
        {
            return expression.AsCharacter();
        }
        public static CharacterMatrix AsCharacterMatrix(this SymbolicExpression expression)
        {
            return expression.AsCharacterMatrix();
        }
        public static ComplexVector AsComplex(this SymbolicExpression expression)
        {
            return expression.AsComplex();
        }
        public static ComplexMatrix AsComplexMatrix(this SymbolicExpression expression)
        {
            return expression.AsComplexMatrix();
        }
        public static DataFrame AsDataFrame(this SymbolicExpression expression)
        {
            return expression.AsDataFrame();
        }
        public static REnvironment AsEnvironment(this SymbolicExpression expression)
        {
            return expression.AsEnvironment();
        }
        public static Expression AsExpression(this SymbolicExpression expression)
        {
            return expression.AsExpression();
        }
        public static Factor AsFactor(this SymbolicExpression expression)
        {
            return expression.AsFactor();
        }
        public static Function AsFunction(this SymbolicExpression expression)
        {
            return expression.AsFunction();
        }
        public static IntegerVector AsInteger(this SymbolicExpression expression)
        {
            return expression.AsInteger();
        }
        public static IntegerMatrix AsIntegerMatrix(this SymbolicExpression expression)
        {
            return expression.AsIntegerMatrix();
        }
        public static Language AsLanguage(this SymbolicExpression expression)
        {
            return expression.AsLanguage();
        }
        public static GenericVector AsList(this SymbolicExpression expression)
        {
            return expression.AsList();
        }
        public static LogicalVector AsLogical(this SymbolicExpression expression)
        {
            return expression.AsLogical();
        }
        public static LogicalMatrix AsLogicalMatrix(this SymbolicExpression expression)
        {
            return expression.AsLogicalMatrix();
        }
        public static NumericVector AsNumeric(this SymbolicExpression expression)
        {
            return expression.AsNumeric();
        }
        public static NumericMatrix AsNumericMatrix(this SymbolicExpression expression)
        {
            return expression.AsNumericMatrix();
        }
        public static RawVector AsRaw(this SymbolicExpression expression)
        {
            return expression.AsRaw();
        }
        public static RawMatrix AsRawMatrix(this SymbolicExpression expression)
        {
            return expression.AsRawMatrix();
        }
        public static S4Object AsS4(this SymbolicExpression expression)
        {
            return expression.AsS4();
        }
        public static Symbol AsSymbol(this SymbolicExpression expression)
        {
            return expression.AsSymbol();
        }
        public static DynamicVector AsVector(this SymbolicExpression expression)
        {
            return expression.AsVector();
        }
        public static bool IsDataFrame(this SymbolicExpression expression)
        {
            return expression.IsDataFrame();
        }
        public static bool IsEnvironment(this SymbolicExpression expression)
        {
            return expression.IsEnvironment();
        }
        public static bool IsExpression(this SymbolicExpression expression)
        {
            return expression.IsExpression();
        }
        public static bool IsFactor(this SymbolicExpression expression)
        {
            return expression.IsFactor();
        }
        public static bool IsFunction(this SymbolicExpression expression)
        {
            return expression.IsFunction();
        }
        public static bool IsLanguage(this SymbolicExpression expression)
        {
            return expression.IsLanguage();
        }
        public static bool IsList(this SymbolicExpression expression)
        {
            return expression.IsList();
        }
        public static bool IsMatrix(this SymbolicExpression expression)
        {
            return expression.IsMatrix();
        }
        public static bool IsS4(this SymbolicExpression expression)
        {
            return expression.IsS4();
        }
        public static bool IsSymbol(this SymbolicExpression expression)
        {
            return expression.IsSymbol();
        }
        public static bool IsVector(this SymbolicExpression expression)
        {
            return expression.IsVector();
        }
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
