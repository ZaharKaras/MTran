// See https://aka.ms/new-console-template for more information
using Python.Core.Abstraction;
using Python.Lexer;
using Python.Parser;

var str = await File.ReadAllTextAsync("D:\\6SEM\\MTran\\MTran\\LexicalAnalyzer\\test1.py");

//var tokens = new PythonLexer(str).Consume();

//PrintTokens(tokens);

PythonParser p = ParsingUnitTest(str);

DateTime parstst = DateTime.UtcNow;
var e38 = p.CompoundSubParser.ParseFunctionDef();

DateTime en = DateTime.UtcNow;
TimeSpan parseoffset = en.Subtract(parstst);
Console.WriteLine("parser time: " + parseoffset);

Console.WriteLine("** done **");


static void PrintTokens(List<Token> tokens)
{
	foreach (Token t in tokens)
	{
		PrintToken(t);
	}
}
static void PrintToken(Token t)
{
	Console.WriteLine("[" + Enum.GetName(typeof(TokenType), t.Type) + "'" + t.Value + "']");
}

static PythonParser ParsingUnitTest(string source)
{
	DateTime st = DateTime.UtcNow;

	PythonParser p = new PythonParser(new PythonLexer(source).Consume());


	DateTime en = DateTime.UtcNow;
	TimeSpan offset = en.Subtract(st);
	Console.WriteLine("tokenizer time: " + offset);

	return p;
}