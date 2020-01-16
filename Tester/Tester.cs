using System;

namespace UOSteam
{
    class Tester
    {
        static void PrintNode(ASTNode node, int indent)
        {
            while (node != null)
            {
                if (node.Lexeme.Length > 0)
                    Console.WriteLine("{0}{1} {2}", "".PadLeft(indent * 4), node.Type, node.Lexeme);
                else
                    Console.WriteLine("{0}{1}", "".PadLeft(indent * 4), node.Type);

                var child = node.FirstChild();

                if (child != null)
                    PrintNode(child, indent + 1);

                node = node.Next();
            }
        }

        static void Main(string[] args)
        {
            var root = Lexer.Lex("test.uos");

            PrintNode(root, 0);

            Commands.RegisterDummyCommands();

            Script script = new Script(root);

            Interpreter.StartScript(script);

            while (Interpreter.ExecuteScripts()) { };

            Console.WriteLine("Done!");
        }
    }
}
