namespace Python.Core.Abstraction
{
	public enum TokenType
	{
		KeyWord, BeginBlock, Number, String,
		BeginParameters, EndParameters, BeginList, EndList,
		ObjectReference, Variable, Operator, ElementSeparator,
		Formatted, Bytes, Decorator, Str, Int, DictionaryStart,
		DictionaryEnd, IndentTab, DedentTab, Tab, Comment, ReturnHint,
		EndOfExpression
	}
}
