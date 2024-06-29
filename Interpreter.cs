﻿using AST;
using static AST.Expr;

namespace CSharpLox
{
    public class Interpreter : IVisitor<object>
    {
        public void Interpret(Expr expression)
        {
            try
            {
                object value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError ex)
            {
                Lox.RuntimeError(ex);
            }
        }

        public class RuntimeError(Token token, string message) : Exception
        {
            public readonly Token token = token;
            public readonly string message = message;
        }

        public object Visit(Binary binary)
        {
            object left = Evaluate(binary.left);
            object right = Evaluate(binary.right);
            
            switch (binary.oper.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left <= (double)right;
                case TokenType.PLUS:
                    if (left is string && right is string) 
                        return (string)left + (string)right;
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    break;
                case TokenType.MINUS:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left - (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left * (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left / (double)right;
                case TokenType.QUESTION_MARK:
                    return Terminal(binary);
                case TokenType.BANG_EQUAL:
                    return !IsEqual(binary.left, binary.right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(binary.left, binary.right);
            }

            return null;
        }

        public object Visit(Grouping grouping)
        {
            return Evaluate(grouping.expression);
        }

        public object Visit(Literal literal)
        {
            return literal.value;
        }

        public object Visit(Unary unary)
        {
            object right = Evaluate(unary);

            switch (unary.oper.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(unary.oper, right);
                    return - (double) right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }

            return null;
        }

        public object Visit(Nothing nothing)
        {
            return "Nothing";
        }

        object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        object Terminal(Binary binaryExpr)
        {
            object condition = Evaluate(binaryExpr.left);

            if (binaryExpr.right is not Binary) throw new RuntimeError(binaryExpr.oper, "Unexpected expression to the right of '?'");

            Binary results = (Binary)binaryExpr.right;

            if ((bool)condition)
                return Evaluate(results.left);

            return Evaluate(results.right);
        }

        static bool IsTruthy(object right)
        {
            if (right == null) return false;
            if (right.GetType() == typeof(bool)) return (bool) right;
            return true;
        }

        static bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;

            return left == right;
        }

        static void CheckNumberOperand(Token oper, object operand)
        {
            if (operand is double) return;

            throw new RuntimeError(oper, "Operand must be a number");
        }

        static void CheckNumberOperands(Token oper, object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(oper, "Both operands must be numbers");
        }

        static string Stringify(object value)
        {
            if (value == null) return "nil";
            return Convert.ToString(value);
        }
    }
}