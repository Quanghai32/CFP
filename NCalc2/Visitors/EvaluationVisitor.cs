﻿using System;
using System.Collections;
using System.Collections.Generic;
using NCalc2.Expressions;

namespace NCalc2.Visitors
{
    public class EvaluationVisitor : LogicalExpressionVisitor
    {
        private readonly EvaluateOptions _options = EvaluateOptions.None;

        private bool IgnoreCase { get { return (_options & EvaluateOptions.IgnoreCase) == EvaluateOptions.IgnoreCase; } }

        public EvaluationVisitor(EvaluateOptions options)
        {
            _options = options;
        }

        public object Result { get; private set; }

        private object Evaluate(LogicalExpression expression)
        {
            expression.Accept(this);
            return Result;
        }

        /// <summary>
        /// Gets the the most precise type.
        /// </summary>
        /// <param name="a">Type a.</param>
        /// <param name="b">Type b.</param>
        /// <returns></returns>
        private static Type GetMostPreciseType(Type a, Type b)
        {
            foreach (Type t in new[] { typeof(String), typeof(Decimal), typeof(Double), typeof(Int32), typeof(Boolean) })
            {
                if (a == t || b == t)
                {
                    return t;
                }
            }

            return a;
        }

        public int CompareUsingMostPreciseType(object a, object b)
        {
            Type mpt = GetMostPreciseType(a.GetType(), b.GetType());
            return Comparer.Default.Compare(Convert.ChangeType(a, mpt), Convert.ChangeType(b, mpt));
        }

        public override void Visit(TernaryExpression expression)
        {
            try
            {
                // Evaluates the left expression and saves the value
                expression.LeftExpression.Accept(this);
                bool left = Convert.ToBoolean(Result);

                if (left)
                {
                    expression.MiddleExpression.Accept(this);
                }
                else
                {
                    expression.RightExpression.Accept(this);
                }
            }
            catch(Exception ex)
            {
                Result = "Error: " + ex.Message;
            }
            finally
            {
                //Saving result
                expression.objResult = Result;
            }
        }

        public override void Visit(BinaryExpression expression)
        {
            // Evaluates the left expression and saves the value
            expression.LeftExpression.Accept(this);
            dynamic left = Result;

            try
            {
                if (expression.Type == BinaryExpressionType.And && !left)
                {
                    Result = false;
                    return;
                }

                if (expression.Type == BinaryExpressionType.Or && left)
                {
                    Result = true;
                    return;
                }
            }
            catch(Exception ex)
            {
                Result = "Error: " + ex.Message;
            }
            finally
            {
                //Saving result
                expression.objResult = Result;
            }

            // Evaluates the right expression and saves the value
            expression.RightExpression.Accept(this);
            dynamic right = Result;
            try
            {
                switch (expression.Type)
                {
                    case BinaryExpressionType.And:
                        Result = left && right;
                        break;

                    case BinaryExpressionType.Or:
                        Result = left || right;
                        break;

                    case BinaryExpressionType.Div:
                        Result = Convert.ToDecimal(left) / Convert.ToDecimal(right);
                        break;

                    case BinaryExpressionType.Equal:
                        Result = left == right;
                        break;

                    case BinaryExpressionType.Greater:
                        Result = left > right;
                        break;

                    case BinaryExpressionType.GreaterOrEqual:
                        Result = left >= right;
                        break;

                    case BinaryExpressionType.Lesser:
                        Result = left < right;
                        break;

                    case BinaryExpressionType.LesserOrEqual:
                        Result = left <= right;
                        break;

                    case BinaryExpressionType.Minus:
                        Result = Convert.ToDecimal(left) - Convert.ToDecimal(right);
                        break;

                    case BinaryExpressionType.Modulo:
                        Result = left % right;
                        break;

                    case BinaryExpressionType.NotEqual:
                        Result = left != right;
                        break;

                    case BinaryExpressionType.Plus:
                        if ((left.GetType() == typeof(string)) && (right.GetType() == typeof(string))) //Add 2 string
                        {
                            Result = left.ToString() + right.ToString();
                        }
                        else if ((left.GetType() == typeof(char)) && (right.GetType() == typeof(char))) //Add 2 char
                        {
                            Result = left.ToString() + right.ToString();
                        }
                        else //Defaul tis add 2 decimal type
                        {
                            Result = Convert.ToDecimal(left) + Convert.ToDecimal(right);
                        }
                        break;

                    case BinaryExpressionType.Times:
                        Result = Convert.ToDecimal(left) * Convert.ToDecimal(right);
                        break;

                    case BinaryExpressionType.BitwiseAnd:
                        Result = left & right;
                        break;

                    case BinaryExpressionType.BitwiseOr:
                        Result = left | right;
                        break;

                    case BinaryExpressionType.BitwiseXOr:
                        Result = left ^ right;
                        break;

                    case BinaryExpressionType.LeftShift:
                        Result = left << right;
                        break;

                    case BinaryExpressionType.RightShift:
                        Result = left >> right;
                        break;
                }
            }
            catch (Exception e)
            {
                //throw new InvalidOperationException(e.Message);
                Result = "Error: " + e.Message;
            }
            finally
            {
                //Saving result
                expression.objResult = Result;
            }
        }

        public override void Visit(UnaryExpression expression)
        {
            try
            {
                // Recursively evaluates the underlying expression
                expression.Expression.Accept(this);

                switch (expression.Type)
                {
                    case UnaryExpressionType.Not:
                        Result = !(dynamic)Result;
                        break;

                    case UnaryExpressionType.Negate:
                        Result = 0 - (dynamic)Result;
                        break;

                    case UnaryExpressionType.BitwiseNot:
                        Result = ~(dynamic)Result;
                        break;
                }
            }
            catch(Exception ex)
            {
                Result = "Error: " + ex.Message;
            }
            finally
            {
                //Saving result
                expression.objResult = Result;
            }
        }

        public override void Visit(ValueExpression expression)
        {
            try
            {
                Result = expression.Value;
            }
            catch (Exception ex)
            {
                Result = "Error: " + ex.Message;
            }
            finally
            {
                //Saving result
                expression.objResult = Result;
            }
        }

        public override void Visit(FunctionExpression function)
        {
            try
            {
                var args = new FunctionArgs();

                if (function.Expressions == null) //Modify for some user function with no parameter like "Empty()" or "NewLine()"...
                {
                    args = new FunctionArgs
                    {
                        Parameters = null
                    };
                }
                else
                {
                    args = new FunctionArgs
                    {
                        Parameters = new Expression[function.Expressions.Length]
                    };


                    // Don't call parameters right now, instead let the function do it as needed.
                    // Some parameters shouldn't be called, for instance, in a if(), the "not" value might be a division by zero
                    // Evaluating every value could produce unexpected behaviour
                    for (int i = 0; i < function.Expressions.Length; i++)
                    {
                        args.Parameters[i] = new Expression(function.Expressions[i], _options);
                        args.Parameters[i].EvaluateFunction += EvaluateFunction;
                        args.Parameters[i].EvaluateParameter += EvaluateParameter;

                        // Assign the parameters of the Expression to the arguments so that custom Functions and Parameters can use them
                        args.Parameters[i].Parameters = Parameters;
                    }

                }

                //Passing parameter
                args.objCommandGuider = function.objCommandGuider;

                // Calls external implementation
                OnEvaluateFunction(IgnoreCase ? function.Identifier.Name.ToLower() : function.Identifier.Name, args);

                // If an external implementation was found get the result back
                if (args.HasResult)
                {
                    Result = args.Result;

                    //Record result
                    function.objResult = Result;
                    //
                    return;
                }

                switch (function.Identifier.Name.ToLower())
                {
                    #region Abs

                    case "abs":

                        //CheckCase("Abs", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Abs() takes exactly 1 argument");

                        Result = Math.Abs(Convert.ToDecimal(
                            Evaluate(function.Expressions[0]))
                            );

                        break;

                    #endregion Abs

                    #region Acos

                    case "acos":

                        //CheckCase("Acos", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Acos() takes exactly 1 argument");

                        Result = Math.Acos(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Acos

                    #region Asin

                    case "asin":

                        //CheckCase("Asin", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Asin() takes exactly 1 argument");

                        Result = Math.Asin(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Asin

                    #region Atan

                    case "atan":

                        //CheckCase("Atan", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Atan() takes exactly 1 argument");

                        Result = Math.Atan(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Atan

                    #region Ceiling

                    case "ceiling":

                        //CheckCase("Ceiling", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Ceiling() takes exactly 1 argument");

                        Result = Math.Ceiling(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Ceiling

                    #region Cos

                    case "cos":

                        //CheckCase("Cos", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Cos() takes exactly 1 argument");

                        Result = Math.Cos(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Cos

                    #region Exp

                    case "exp":

                        //CheckCase("Exp", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Exp() takes exactly 1 argument");

                        Result = Math.Exp(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Exp

                    #region Floor

                    case "floor":

                        //CheckCase("Floor", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Floor() takes exactly 1 argument");

                        Result = Math.Floor(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Floor

                    #region IEEERemainder

                    case "ieeeremainder":

                        //CheckCase("IEEERemainder", function.Identifier.Name);

                        if (function.Expressions.Length != 2)
                            throw new ArgumentException("IEEERemainder() takes exactly 2 arguments");

                        Result = Math.IEEERemainder(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToDouble(Evaluate(function.Expressions[1])));

                        break;

                    #endregion IEEERemainder

                    #region Log

                    case "log":

                        //CheckCase("Log", function.Identifier.Name);

                        if (function.Expressions.Length != 2)
                            throw new ArgumentException("Log() takes exactly 2 arguments");

                        Result = Math.Log(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToDouble(Evaluate(function.Expressions[1])));

                        break;

                    #endregion Log

                    #region Log10

                    case "log10":

                        //CheckCase("Log10", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Log10() takes exactly 1 argument");

                        Result = Math.Log10(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Log10

                    #region Pow

                    case "pow":

                        //CheckCase("Pow", function.Identifier.Name);

                        if (function.Expressions.Length != 2)
                            throw new ArgumentException("Pow() takes exactly 2 arguments");

                        Result = Math.Pow(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToDouble(Evaluate(function.Expressions[1])));

                        break;

                    #endregion Pow

                    #region Round

                    case "round":

                        //CheckCase("Round", function.Identifier.Name);

                        if (function.Expressions.Length != 2)
                            throw new ArgumentException("Round() takes exactly 2 arguments");

                        MidpointRounding rounding = (_options & EvaluateOptions.RoundAwayFromZero) == EvaluateOptions.RoundAwayFromZero ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven;

                        Result = Math.Round(Convert.ToDouble(Evaluate(function.Expressions[0])), Convert.ToInt16(Evaluate(function.Expressions[1])), rounding);

                        break;

                    #endregion Round

                    #region Sign

                    case "sign":

                        //CheckCase("Sign", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Sign() takes exactly 1 argument");

                        Result = Math.Sign(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Sign

                    #region Sin

                    case "sin":

                        //CheckCase("Sin", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Sin() takes exactly 1 argument");

                        Result = Math.Sin(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Sin

                    #region Sqrt

                    case "sqrt":

                        //CheckCase("Sqrt", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Sqrt() takes exactly 1 argument");

                        Result = Math.Sqrt(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Sqrt

                    #region Tan

                    case "tan":

                        //CheckCase("Tan", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Tan() takes exactly 1 argument");

                        Result = Math.Tan(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Tan

                    #region Truncate

                    case "truncate":

                        //CheckCase("Truncate", function.Identifier.Name);

                        if (function.Expressions.Length != 1)
                            throw new ArgumentException("Truncate() takes exactly 1 argument");

                        Result = Math.Truncate(Convert.ToDouble(Evaluate(function.Expressions[0])));

                        break;

                    #endregion Truncate

                    #region Max

                    case "max":

                        //CheckCase("Max", function.Identifier.Name);

                        if (function.Expressions.Length != 2)
                            throw new ArgumentException("Max() takes exactly 2 arguments");

                        dynamic maxleft = Evaluate(function.Expressions[0]);
                        dynamic maxright = Evaluate(function.Expressions[1]);

                        Result = Math.Max(maxleft, maxright);
                        break;

                    #endregion Max

                    #region Min

                    case "min":

                        //CheckCase("Min", function.Identifier.Name);

                        if (function.Expressions.Length != 2)
                            throw new ArgumentException("Min() takes exactly 2 arguments");

                        dynamic minleft = Evaluate(function.Expressions[0]);
                        dynamic minright = Evaluate(function.Expressions[1]);

                        Result = Math.Min(minleft, minright);
                        break;

                    #endregion Min

                    #region if

                    case "if":

                        //CheckCase("if", function.Identifier.Name);

                        if (function.Expressions.Length < 2)
                            throw new ArgumentException("if() require at least 2 arguments");

                        bool cond = Convert.ToBoolean(Evaluate(function.Expressions[0]));

                        if (function.Expressions.Length < 3) //There is only 2
                        {
                            if (cond) Evaluate(function.Expressions[1]);
                        }
                        else //enough 3 parameter
                        {
                            Result = cond ? Evaluate(function.Expressions[1]) : Evaluate(function.Expressions[2]);
                        }
                        break;

                    #endregion if

                    #region in

                    case "in":

                        //CheckCase("in", function.Identifier.Name);

                        if (function.Expressions.Length < 2)
                            throw new ArgumentException("in() takes at least 2 arguments");

                        object parameter = Evaluate(function.Expressions[0]);

                        bool evaluation = false;

                        // Goes through any values, and stop whe one is found
                        for (int i = 1; i < function.Expressions.Length; i++)
                        {
                            object argument = Evaluate(function.Expressions[i]);
                            if (CompareUsingMostPreciseType(parameter, argument) == 0)
                            {
                                evaluation = true;
                                break;
                            }
                        }

                        Result = evaluation;
                        break;

                    #endregion in

                    default:
                        //throw new ArgumentException("Function not found",
                        //    function.Identifier.Name);
                        Result = "Error: Function not found [" + function.Identifier.Name + "]";
                        break;
                }
            }
            catch(Exception ex)
            {
                Result = "Error: " + ex.Message;
            }
            finally
            {
                //Record result
                function.objResult = Result;
            }
        }

        private void CheckCase(string function, string called)
        {
            if (IgnoreCase)
            {
                if (function.ToLower() == called.ToLower())
                {
                    return;
                }

                throw new ArgumentException("Function not found", called);
            }

            if (function != called)
            {
                throw new ArgumentException(String.Format("Function not found {0}. Try {1} instead.", called, function));
            }
        }

        public event EvaluateFunctionHandler EvaluateFunction;

        private void OnEvaluateFunction(string name, FunctionArgs args)
        {
            if (EvaluateFunction != null)
                EvaluateFunction(name, args);
        }

        public override void Visit(IdentifierExpression parameter)
        {
            try
            {
                if (Parameters.ContainsKey(parameter.Name))
                {
                    // The parameter is defined in the hashtable
                    if (Parameters[parameter.Name] is Expression)
                    {
                        // The parameter is itself another Expression
                        var expression = (Expression)Parameters[parameter.Name];

                        // Overloads parameters
                        foreach (var p in Parameters)
                        {
                            expression.Parameters[p.Key] = p.Value;
                        }

                        expression.EvaluateFunction += EvaluateFunction;
                        expression.EvaluateParameter += EvaluateParameter;

                        Result = ((Expression)Parameters[parameter.Name]).Evaluate();
                    }
                    else
                        Result = Parameters[parameter.Name];
                }
                else
                {
                    // The parameter should be defined in a call back method
                    var args = new ParameterArgs();

                    // Calls external implementation
                    OnEvaluateParameter(parameter.Name, args);

                    //if (!args.HasResult)
                    //    throw new ArgumentException("Parameter was not defined", parameter.Name);
                    if (args.HasResult)
                    {
                        Result = args.Result;
                    }
                    else
                    {
                        Result = parameter.Name;
                    }
                }
            }
            catch(Exception ex)
            {
                Result = "Error: " + ex.Message;
            }
            finally
            {
                //saving result
                parameter.objResult = Result;
            }
        }

        public event EvaluateParameterHandler EvaluateParameter;

        private void OnEvaluateParameter(string name, ParameterArgs args)
        {
            if (EvaluateParameter != null)
                EvaluateParameter(name, args);
        }

        public Dictionary<string, object> Parameters { get; set; }
    }
}