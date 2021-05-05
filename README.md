This is an implementation of the UO Steam scripting language engine. The interpreter is not tied to any particular program, but is instead extensible through the registration of command handlers.

To run a test of the interpreter, first build it. The output will consist of a library named UOSteam.dll and an application called Tester.exe. The Tester.exe application currently expects a file named "test.uos" containing a Steam script in the same directory as the executable. You can try various valid Steam scripts and it will first print the parsed abstract syntax tree for the script, then execute it.

At this point, this project is considered complete. It supports all of the language features that the steam scripting language supported, plus some additional ones. This project has is used by the following projects:

* Razor Community Edition - Implements a different set of commands than Steam
* Razor Enhanced - Attempts to implement the exact same commands as Steam

Steam Script Overview
----------------------

The full documentation for the Steam scripting language is located [here](https://github.com/her/uosteam/blob/master/uos/UOSteamDocumentation.pdf). While that outlines the valid syntax, the following will break the language down at a more technical level. Here is an example Steam script.

~~~
msg 'Hello'
if @findtype '0x1bdd' 'any' 'backpack' 'any' 'any'
  @setalias 'Logs' 'Found'
  useobject 'Axe'
  autotargetobject 'Logs'
  pause 1000
endif
~~~

Most lines in a Steam script are called `Commands`. They simply execute some action (e.g. `msg 'Hello'`). The first word (ignoring modifiers such as `@`) represents the command and the remaining tokens are the arguments to that command.

Some lines instead contain control-flow statements such as `if`, `for`, `while`, and their associated end-statements like `endif`. Following a control-flow statement comes an `Expression`. In the above example, `@findtype '0x1bdd' 'any' 'backpack' 'any' 'any'` is an expression. Expressions come in two types - unary and binary. The previous example is a unary expression. A binary expression contains a comparison operator such as `hits < 100`. For unary expressions, the first token (ignoring modifiers such as `@`) in the expression is the expression type and the remaining tokens are arguments. For binary expressions, everything is treated as an alias or value.

The final major primitive is an `Alias`. Aliases are human-readable names for values, such as `any` and `backpack`. An alias always evaluates to a value. Many aliases always exist when a Steam script runs, but additionally a user may create their own aliases using the `setalias` command. Users may only set aliases to serial numbers - not to any other value type.

Design
------

The solution consists of two projects. `Tester` is a test application that runs steam scripts for testing purposes. `UOSteam` is an assembly that contains the actual interpreter.

The UOSteam interpreter itself is only two files:

* Lexer.cs contains the Lexer (sometimes called a parser). The Lexer class has a single public function `Lex` that takes the raw script input and parses it into an [Abstract Syntax Tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree). It returns the root of the tree.
* Interpreter.cs contains two classes. The `Script` class represents one Steam script. The constructor requires the root node of an abstract syntax tree. The `Interpreter` class represents an entire interpreter. This is necessary because some aliases are global - their values are visible across all scripts executing now and into the future.

Neither the interpreter nor the parser actually know which `Commands`, `Expressions`, and `Aliases` are actually valid. Instead, the `Interpreter` class has methods for registering handlers for any command, expression, and alias necessary.

The `Tester` application contains a file called `Commands.cs` that registers handlers for all of the standard Steam `Commands`, `Expressions`, and `Aliases`. This handler, however, simply prints to the console so that we know they executed. When integrating this Steam interpreter into a real application, you're expected to register your own handlers that take the appropriate actions.
