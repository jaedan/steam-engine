using System;
using System.Collections.Generic;

namespace UOSteam
{
    // This registers default handlers for all of the commands and expressions
    // so valid scripts will at least run for testing.
    public static class Commands
    {
        private static int DummyExpression(ref ASTNode node, bool quiet)
        {
            Console.WriteLine("Executing expression {0} {1}", node.Type, node.Lexeme);

            while (node != null)
            {
                switch (node.Type)
                {
                    case ASTNodeType.EQUAL:
                    case ASTNodeType.NOT_EQUAL:
                    case ASTNodeType.LESS_THAN:
                    case ASTNodeType.LESS_THAN_OR_EQUAL:
                    case ASTNodeType.GREATER_THAN:
                    case ASTNodeType.GREATER_THAN_OR_EQUAL:
                        return 0;
                }

                node = node.Next();
            }

            return 0;
        }

        private static void DummyCommand(ref ASTNode node, bool quiet, bool force)
        {
            Console.WriteLine("Executing command {0} {1}", node.Type, node.Lexeme);

            node = null;
        }

        static Commands()
        {
            // Commands. From UOSteam Documentation
            Interpreter.RegisterCommandHandler("fly", DummyCommand);
            Interpreter.RegisterCommandHandler("land", DummyCommand);
            Interpreter.RegisterCommandHandler("setability", DummyCommand);
            Interpreter.RegisterCommandHandler("attack", DummyCommand);
            Interpreter.RegisterCommandHandler("clearhands", DummyCommand);
            Interpreter.RegisterCommandHandler("clickobject", DummyCommand);
            Interpreter.RegisterCommandHandler("bandageself", DummyCommand);
            Interpreter.RegisterCommandHandler("usetype", DummyCommand);
            Interpreter.RegisterCommandHandler("useobject", DummyCommand);
            Interpreter.RegisterCommandHandler("useonce", DummyCommand);
            Interpreter.RegisterCommandHandler("cleanusequeue", DummyCommand);
            Interpreter.RegisterCommandHandler("moveitem", DummyCommand);
            Interpreter.RegisterCommandHandler("moveitemoffset", DummyCommand);
            Interpreter.RegisterCommandHandler("movetype", DummyCommand);
            Interpreter.RegisterCommandHandler("movetypeoffset", DummyCommand);
            Interpreter.RegisterCommandHandler("walk", DummyCommand);
            Interpreter.RegisterCommandHandler("turn", DummyCommand);
            Interpreter.RegisterCommandHandler("run", DummyCommand);
            Interpreter.RegisterCommandHandler("useskill", DummyCommand);
            Interpreter.RegisterCommandHandler("feed", DummyCommand);
            Interpreter.RegisterCommandHandler("rename", DummyCommand);
            Interpreter.RegisterCommandHandler("shownames", DummyCommand);
            Interpreter.RegisterCommandHandler("togglehands", DummyCommand);
            Interpreter.RegisterCommandHandler("equipitem", DummyCommand);
            Interpreter.RegisterCommandHandler("togglemounted", DummyCommand);
            Interpreter.RegisterCommandHandler("equipwand", DummyCommand);
            Interpreter.RegisterCommandHandler("buy", DummyCommand);
            Interpreter.RegisterCommandHandler("sell", DummyCommand);
            Interpreter.RegisterCommandHandler("clearbuy", DummyCommand);
            Interpreter.RegisterCommandHandler("clearsell", DummyCommand);
            Interpreter.RegisterCommandHandler("organizer", DummyCommand);
            Interpreter.RegisterCommandHandler("autoloot", DummyCommand);
            Interpreter.RegisterCommandHandler("dress", DummyCommand);
            Interpreter.RegisterCommandHandler("undress", DummyCommand);
            Interpreter.RegisterCommandHandler("dressconfig", DummyCommand);
            Interpreter.RegisterCommandHandler("toggleautoloot", DummyCommand);
            Interpreter.RegisterCommandHandler("togglescavenger", DummyCommand);
            Interpreter.RegisterCommandHandler("counter", DummyCommand);
            Interpreter.RegisterCommandHandler("unsetalias", DummyCommand);
            Interpreter.RegisterCommandHandler("setalias", DummyCommand);
            Interpreter.RegisterCommandHandler("promptalias", DummyCommand);

            // Expressions
            Interpreter.RegisterExpressionHandler("findalias", DummyExpression);


            // Aliases
            backpack
            bank
            enemy
            friend
            ground
            last
            lasttarget
            lastobject
            lefthand
            mount
            righthand
            self

        }
    }
}