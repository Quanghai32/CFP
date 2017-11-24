using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Antlr4.Runtime;
using NCalc2.Expressions;
using NCalc2.Grammar;
using NCalc2.Visitors;

namespace NCalc2
{
    public class Expression
    {
        public EvaluateOptions Options { get; set; }

        /// <summary>
        /// Textual representation of the expression to evaluate.
        /// </summary>
        protected string OriginalExpression;

        public Expression()
        {
        }

        public Expression(string expression)
            : this(expression, EvaluateOptions.None)
        {
        }

        public Expression(string expression, EvaluateOptions options)
        {
            if (String.IsNullOrEmpty(expression))
                throw new
                    ArgumentException("Expression can't be empty", "expression");

            OriginalExpression = expression;
            Options = options;
        }

        public Expression(LogicalExpression expression, EvaluateOptions options)
        {
            if (expression == null)
                throw new
                    ArgumentException("Expression can't be null", "expression");

            ParsedExpression = expression;
            Options = options;
        }

        public LogicalExpression Compile(string expression, bool nocache)
        {
            if (expression == null) return null;
            LogicalExpression logicalExpression = null;

            //NCalc has problem if user function with no parameter such as "Empty()" , "NewLine()"...
            //We temporary add "null" to these: "Empty()" => "Empty(null)". "NewLine()" => "NewLine(null)"...
            //After finish analyzing, return expression to null
            bool blMasking = false;
            if (expression.Contains("()")==true) //Mother expression or child expression
            {
                blMasking = true;
                expression = expression.Replace("()", "(null)"); //Do masking
            }
            

            if (logicalExpression == null)
            {
                var lexer = new NCalc2Lexer(new AntlrInputStream(expression));
                var parser = new NCalc2Parser(new CommonTokenStream(lexer));
                var errorListener = new ErrorListener();
                parser.AddErrorListener(errorListener);
                logicalExpression = parser.ncalc().retValue;


                if (blMasking==true)
                {
                    //In case of User function with no parameter or have child expression with no parameter
                    //if which expression no parameter => return to null 
                    logicalExpression = RemoveNullParameterExpression(logicalExpression);
                }

               
                if (errorListener.Errors.Any())
                {
                    //throw new EvaluationException(string.Join(Environment.NewLine, errorListener.Errors.ToArray()));
                    //In case of error, then return a logical expression with "expression" same as input string
                    logicalExpression = new ValueExpression(expression, Expressions.ValueType.String);
                }

            }

            return logicalExpression;
        }

        public LogicalExpression AnalyzeExpression(string expression)
        {
            Options = EvaluateOptions.IgnoreCase;
            if (String.IsNullOrEmpty(expression))
            {
                return null;
            }
            LogicalExpression lgRet = Compile(expression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
            
            //Return
            return lgRet;
        }

        public LogicalExpression RemoveNullParameterExpression(LogicalExpression lgRet)
        {
            //separate case
            if (lgRet is TernaryExpression)
            {
                TernaryExpression TernaryEx = (TernaryExpression)lgRet;

                if (TernaryEx.LeftExpression != null)
                {
                    TernaryEx.LeftExpression = this.RemoveNullParameterExpression(TernaryEx.LeftExpression);
                }

                if (TernaryEx.MiddleExpression != null)
                {
                    TernaryEx.MiddleExpression = this.RemoveNullParameterExpression(TernaryEx.MiddleExpression);
                }

                if (TernaryEx.RightExpression != null)
                {
                    TernaryEx.RightExpression = this.RemoveNullParameterExpression(TernaryEx.RightExpression);
                }

            }
            else if (lgRet is BinaryExpression)
            {
                BinaryExpression BinaryEx = (BinaryExpression)lgRet;

                if (BinaryEx.LeftExpression != null)
                {
                    BinaryEx.LeftExpression = this.RemoveNullParameterExpression(BinaryEx.LeftExpression);
                }

                if (BinaryEx.RightExpression != null)
                {
                    BinaryEx.RightExpression = this.RemoveNullParameterExpression(BinaryEx.RightExpression);
                }
            }
            else if (lgRet is UnaryExpression)
            {
                UnaryExpression UnaryEx = (UnaryExpression)lgRet;

                if (UnaryEx.Expression != null)
                {
                    UnaryEx.Expression = this.RemoveNullParameterExpression(UnaryEx.Expression);
                }
            }
            else if (lgRet is ValueExpression) //Break point of recursive 
            {
                ValueExpression ValueEx = (ValueExpression)lgRet;
            }
            else if (lgRet is FunctionExpression)
            {
                FunctionExpression FunctionEx = (FunctionExpression)lgRet;
                FunctionEx = (FunctionExpression)this.RemoveNullParameter(lgRet);

                if (FunctionEx.Expressions != null)
                {
                    for (int i = 0; i < FunctionEx.Expressions.Length; i++)
                    {
                        LogicalExpression item = FunctionEx.Expressions[i];

                        if (item != null)
                        {
                            item = this.RemoveNullParameterExpression(item);
                        }
                    }
                }
            }
            else if (lgRet is IdentifierExpression)  //Break point of recursive 
            {
                IdentifierExpression IdentifierEx = (IdentifierExpression)lgRet;
            }
            else  //Break point of recursive 
            {

            }

            //
            return lgRet;
        }

        public LogicalExpression RemoveNullParameter(LogicalExpression lgRet)
        {
            if (lgRet is FunctionExpression)
            {
                FunctionExpression FuncTemp = (FunctionExpression)lgRet;
                if (FuncTemp.Expressions!= null)
                {
                    if(FuncTemp.Expressions.Length == 1)
                    {
                        var type = FuncTemp.Expressions[0].GetType();
                        if(type == typeof(IdentifierExpression))
                        {
                            //string strTest = "OK";
                            FuncTemp.Expressions[0] = new ValueExpression(string.Empty, Expressions.ValueType.String);
                            lgRet = FuncTemp;
                        }
                    }
                }
            }
            //
            return lgRet;
        }

        /// <summary>
        /// Pre-compiles the expression in order to check syntax errors.
        /// If errors are detected, the Error property contains the message.
        /// </summary>
        /// <returns>True if the expression syntax is correct, otherwiser False</returns>
        public bool HasErrors()
        {
            try
            {
                if (ParsedExpression == null)
                {
                    ParsedExpression = Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
                }

                // In case HasErrors() is called multiple times for the same expression
                return ParsedExpression != null && Error != null;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return true;
            }
        }

        public string Error { get; private set; }

        public LogicalExpression ParsedExpression { get; private set; }

        protected Dictionary<string, IEnumerator> ParameterEnumerators;
        protected Dictionary<string, object> ParametersBackup;

        public object Evaluate(LogicalExpression InputLogicExpression = null)
        {
            if (InputLogicExpression == null) //evaluate with existing logical expression
            {
                if (HasErrors())
                {
                    throw new EvaluationException(Error);
                }

                if (ParsedExpression == null)
                {
                    ParsedExpression = Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
                }
            }
            else //New evaluate
            {
                this.ParsedExpression = InputLogicExpression;
            }

            //
            if (ParsedExpression == null) return null;


            var visitor = new EvaluationVisitor(Options);
            visitor.EvaluateFunction += EvaluateFunction;
            visitor.EvaluateParameter += EvaluateParameter;
            visitor.Parameters = Parameters;

            // if array evaluation, execute the same expression multiple times
            if ((Options & EvaluateOptions.IterateParameters) == EvaluateOptions.IterateParameters)
            {
                int size = -1;
                ParametersBackup = new Dictionary<string, object>();
                foreach (string key in Parameters.Keys)
                {
                    ParametersBackup.Add(key, Parameters[key]);
                }

                ParameterEnumerators = new Dictionary<string, IEnumerator>();

                foreach (object parameter in Parameters.Values)
                {
                    if (parameter is IEnumerable)
                    {
                        int localsize = 0;
                        foreach (object o in (IEnumerable)parameter)
                        {
                            localsize++;
                        }

                        if (size == -1)
                        {
                            size = localsize;
                        }
                        else if (localsize != size)
                        {
                            throw new EvaluationException("When IterateParameters option is used, IEnumerable parameters must have the same number of items");
                        }
                    }
                }

                foreach (string key in Parameters.Keys)
                {
                    var parameter = Parameters[key] as IEnumerable;
                    if (parameter != null)
                    {
                        ParameterEnumerators.Add(key, parameter.GetEnumerator());
                    }
                }

                var results = new List<object>();
                for (int i = 0; i < size; i++)
                {
                    foreach (string key in ParameterEnumerators.Keys)
                    {
                        IEnumerator enumerator = ParameterEnumerators[key];
                        enumerator.MoveNext();
                        Parameters[key] = enumerator.Current;
                    }

                    ParsedExpression.Accept(visitor);
                    results.Add(visitor.Result);
                }

                return results;
            }

            ParsedExpression.Accept(visitor);

            //Record data
            ParsedExpression.objResult = visitor.Result;

            //Resturn data
            return visitor.Result;
        }

        public event EvaluateFunctionHandler EvaluateFunction;

        public event EvaluateParameterHandler EvaluateParameter;

        private Dictionary<string, object> _parameters;

        public Dictionary<string, object> Parameters
        {
            get { return _parameters ?? (_parameters = new Dictionary<string, object>()); }
            set { _parameters = value; }
        }
    }
}