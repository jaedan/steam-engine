using System;
using System.Collections.Generic;

namespace UOSteam
{
    public class Script
    {
        private ASTNode _statement;

        // For-loop indices
        private Dictionary<ASTNode, int> _forloops = new Dictionary<ASTNode, int>();

        // For now, the scripts execute directly from the
        // abstract syntax tree. This is relatively simple.
        // A more robust approach would be to "compile" the
        // scripts to a bytecode. That would allow more errors
        // to be caught with better error messages, as well as
        // make the scripts execute more quickly.
        public Script(ASTNode root)
        {
            // Set current to the first statement
            _statement = root.FirstChild();
        }

        public bool ExecuteNext()
        {
            if (_statement == null)
                return false;

            if (_statement.Type != ASTNodeType.STATEMENT)
                throw new Exception("Invalid script");

            var node = _statement.FirstChild();

            if (node == null)
                throw new Exception("Invalid statement");

            switch (node.Type)
            {
                case ASTNodeType.IF:
                case ASTNodeType.ELSEIF:
                {
                    var result = EvaluateExpression(node.FirstChild());

                    // Advance to next statement
                    _statement = _statement.Next();

                    // The expression evaluated false, so keep advancing until
                    // we hit an elseif or endif statement.
                    if (!result)
                    {
                        while (_statement != null)
                        {
                            node = _statement.FirstChild();

                            if (node.Type == ASTNodeType.ELSEIF ||
                                node.Type == ASTNodeType.ENDIF)
                                break;

                            _statement = _statement.Next();
                        }

                        if (_statement == null)
                            throw new Exception("If with no matching endif");
                    }
                    break;
                }
                case ASTNodeType.ENDIF:
                    _statement = _statement.Next();
                    break;
                case ASTNodeType.WHILE:
                {
                    var result = EvaluateExpression(node.FirstChild());

                    // Advance to next statement
                    _statement = _statement.Next();

                    // The expression evaluated false, so keep advancing until
                    // we hit an endwhile statement.
                    if (!result)
                    {
                        while (_statement != null)
                        {
                            node = _statement.FirstChild();

                            if (node.Type == ASTNodeType.ENDWHILE)
                            {
                                // Go one past the endwhile so the loop doesn't repeat
                                _statement = _statement.Next();
                                break;
                            }

                            _statement = _statement.Next();
                        }
                    }
                    break;
                }
                case ASTNodeType.ENDWHILE:
                    // Walk backward to the while statement
                    _statement = _statement.Prev();

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.WHILE)
                        {
                            break;
                        }

                        _statement = _statement.Prev();
                    }

                    if (_statement == null)
                        throw new Exception("Unexpected endwhile");
                    break;
                case ASTNodeType.FOR:
                    throw new Exception("For loops are not supported yet");
                case ASTNodeType.ENDFOR:
                    // Walk backward to the for statement
                    _statement = _statement.Prev();

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.FOR)
                        {
                            break;
                        }

                        _statement = _statement.Prev();
                    }

                    if (_statement == null)
                        throw new Exception("Unexpected endfor");
                    break;
                case ASTNodeType.BREAK:
                    // Walk until the end of the loop
                    _statement = _statement.Next();

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.ENDWHILE ||
                            node.Type == ASTNodeType.ENDFOR)
                        {
                            // Go one past the end so the loop doesn't repeat
                            _statement = _statement.Next();
                            break;
                        }

                        _statement = _statement.Next();
                    }
                    break;
                case ASTNodeType.CONTINUE:
                    // Walk backward to the loop statement
                    _statement = _statement.Prev();

                    while (_statement != null)
                    {
                        node = _statement.FirstChild();

                        if (node.Type == ASTNodeType.WHILE ||
                            node.Type == ASTNodeType.FOR)
                        {
                            break;
                        }

                        _statement = _statement.Prev();
                    }

                    if (_statement == null)
                        throw new Exception("Unexpected continue");
                    break;
                case ASTNodeType.STOP:
                    _statement = null;
                    break;
                case ASTNodeType.REPLAY:
                    _statement = _statement.Parent.FirstChild();
                    break;
                case ASTNodeType.QUIET:
                case ASTNodeType.FORCE:
                case ASTNodeType.COMMAND:
                    if (ExecuteCommand(node))
                        _statement = _statement.Next();
                    break;
            }

            return (_statement != null) ? true: false;
        }

        private ASTNode EvaluateModifiers(ASTNode node, out bool quiet, out bool force, out bool not)
        {
            quiet = false;
            force = false;
            not = false;

            while (true)
            {
                switch (node.Type)
                {
                    case ASTNodeType.QUIET:
                        quiet = true;
                        break;
                    case ASTNodeType.FORCE:
                        force = true;
                        break;
                    case ASTNodeType.NOT:
                        not = true;
                        break;
                    default:
                        return node;
                }

                node = node.Next();
            }
        }

        private bool ExecuteCommand(ASTNode node)
        {
            node = EvaluateModifiers(node, out bool quiet, out bool force, out _);

            var handler = Interpreter.GetCommandHandler(node.Lexeme);

            if (handler == null)
                throw new Exception("Unknown command");

            var cont = handler(ref node, quiet, force);

            if (node != null)
                throw new Exception("Command did not consume all available arguments");

            return cont;
        }

        private bool EvaluateExpression(ASTNode expr)
        {
            if (expr == null || (expr.Type != ASTNodeType.UNARY_EXPRESSION && expr.Type != ASTNodeType.BINARY_EXPRESSION))
                throw new Exception("No expression following control statement");

            var node = expr.FirstChild();

            if (node == null)
                throw new Exception("Empty expression following control statement");

            if (expr.Type == ASTNodeType.UNARY_EXPRESSION)
                return EvaluateUnaryExpression(node);
            else
                return EvaluateBinaryExpression(node);
        }

        private bool EvaluateUnaryExpression(ASTNode node)
        {
            node = EvaluateModifiers(node, out bool quiet, out _, out bool not);

            // Unary expressions are converted to bool.
            var result = ExecuteExpression(ref node, quiet) != 0;

            if (node != null)
                throw new Exception("Command did not consume all provided arguments");

            if (not)
                return !result;
            else
                return result;
        }

        private bool EvaluateBinaryExpression(ASTNode node)
        {
            int lhs;
            int rhs;

            // Evaluate the left hand side
            node = EvaluateModifiers(node, out bool quiet, out _, out _);
            lhs = ExecuteExpression(ref node, quiet);

            // Capture the operator
            var op = node.Type;
            node = node.Next();

            // Evaluate the right hand side
            node = EvaluateModifiers(node, out quiet, out _, out _);
            rhs = ExecuteExpression(ref node, quiet);

            if (node != null)
                throw new Exception("Command did not consume all provided arguments");

            switch (op)
            {
                case ASTNodeType.EQUAL:
                    return lhs == rhs;
                case ASTNodeType.NOT_EQUAL:
                    return lhs != rhs;
                case ASTNodeType.LESS_THAN:
                    return lhs < rhs;
                case ASTNodeType.LESS_THAN_OR_EQUAL:
                    return lhs <= rhs;
                case ASTNodeType.GREATER_THAN:
                    return lhs > rhs;
                case ASTNodeType.GREATER_THAN_OR_EQUAL:
                    return lhs >= rhs;
            }

            throw new Exception("Invalid operator type in expression");
        }

        private int ExecuteExpression(ref ASTNode node, bool quiet)
        {
            var handler = Interpreter.GetExpressionHandler(node.Lexeme);

            if (handler == null)
                throw new Exception("Unknown expression");

            var result = handler(ref node, quiet);

            return result;
        }
    }

    public static class Interpreter
    {
        // Aliases only hold serial numbers
        private static Dictionary<string, int> _aliases = new Dictionary<string, int>();

        // Lists
        private static Dictionary<string, object[]> _lists = new Dictionary<string, object[]>();

        public delegate int ExpressionHandler(ref ASTNode node, bool quiet);

        private static Dictionary<string, ExpressionHandler> _exprHandlers = new Dictionary<string, ExpressionHandler>();

        public delegate bool CommandHandler(ref ASTNode node, bool quiet, bool force);

        private static Dictionary<string, CommandHandler> _commandHandlers = new Dictionary<string, CommandHandler>();

        public delegate int AliasHandler(ref ASTNode node);

        private static Dictionary<string, AliasHandler> _aliasHandlers = new Dictionary<string, AliasHandler>();

        private static LinkedList<Script> _scripts = new LinkedList<Script>();

        public static void RegisterExpressionHandler(string keyword, ExpressionHandler handler)
        {
            _exprHandlers[keyword] = handler;
        }

        public static ExpressionHandler GetExpressionHandler(string keyword)
        {
            _exprHandlers.TryGetValue(keyword, out ExpressionHandler handler);

            return handler;
        }

        public static void RegisterCommandHandler(string keyword, CommandHandler handler)
        {
            _commandHandlers[keyword] = handler;
        }

        public static CommandHandler GetCommandHandler(string keyword)
        {
            _commandHandlers.TryGetValue(keyword, out CommandHandler handler);

            return handler;
        }

        public static void RegisterAliasHandler(string keyword, AliasHandler handler)
        {
            _aliasHandlers[keyword] = handler;
        }

        public static int GetAlias(ref ASTNode node)
        {
            // If a handler is explicitly registered, call that.
            if (_aliasHandlers.TryGetValue(node.Lexeme, out AliasHandler handler))
                return handler(ref node);

            int value;
            if (_aliases.TryGetValue(node.Lexeme, out value))
                return value;

            return -1;
        }

        public static void SetAlias(string alias, int serial)
        {
            _aliases[alias] = serial;
        }

        public static void StartScript(Script script)
        {
            _scripts.AddLast(script);
        }

        public static void StopScript(Script script)
        {
            _scripts.Remove(script);
        }

        public static bool ExecuteScripts()
        {
            var node = _scripts.Last;

            while (node != null)
            {
                node.Value.ExecuteNext();

                node = node.Previous;
            }

            return _scripts.Count > 0;
        }
    }
}