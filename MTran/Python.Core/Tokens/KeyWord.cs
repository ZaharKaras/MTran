namespace Python.Core.Tokens
{
	public class KeyWord
	{
		public static readonly KeyWord And = new KeyWord("and", false, false);
		public static readonly KeyWord As = new KeyWord("as", false, false);
		public static readonly KeyWord Assert = new KeyWord("assert", false, false);
		public static readonly KeyWord Async = new KeyWord("async", false, false);
		public static readonly KeyWord Await = new KeyWord("await", false, false);
		public static readonly KeyWord Break = new KeyWord("break", false, false);
		public static readonly KeyWord Class = new KeyWord("class", true, false);
		public static readonly KeyWord Continue = new KeyWord("continue", false, false);
		public static readonly KeyWord Def = new KeyWord("def", true, false);
		public static readonly KeyWord Del = new KeyWord("del", false, false);
		public static readonly KeyWord Elif = new KeyWord("elif", true, true);
		public static readonly KeyWord Else = new KeyWord("else", true, true);
		public static readonly KeyWord Except = new KeyWord("except", true, false);
		public static readonly KeyWord False = new KeyWord("False", false, false);
		public static readonly KeyWord Finally = new KeyWord("finally", true, false);
		public static readonly KeyWord For = new KeyWord("for", true, false);
		public static readonly KeyWord From = new KeyWord("from", false, false);
		public static readonly KeyWord Global = new KeyWord("global", false, false);
		public static readonly KeyWord If = new KeyWord("if", true, true);
		public static readonly KeyWord Import = new KeyWord("import", false, false);
		public static readonly KeyWord In = new KeyWord("in", false, false);
		public static readonly KeyWord Is = new KeyWord("is", false, false);
		public static readonly KeyWord Lambda = new KeyWord("lambda", true, false);
		public static readonly KeyWord None = new KeyWord("None", false, false);
		public static readonly KeyWord Nonlocal = new KeyWord("nonlocal", false, false);
		public static readonly KeyWord Not = new KeyWord("not", false, false);
		public static readonly KeyWord Or = new KeyWord("or", false, false);
		public static readonly KeyWord Pass = new KeyWord("pass", false, false);
		public static readonly KeyWord Raise = new KeyWord("raise", false, false);
		public static readonly KeyWord Return = new KeyWord("return", false, false);
		public static readonly KeyWord True = new KeyWord("True", false, false);
		public static readonly KeyWord Try = new KeyWord("try", true, false);
		public static readonly KeyWord While = new KeyWord("while", true, true);
		public static readonly KeyWord With = new KeyWord("with", false, false);
		public static readonly KeyWord Yield = new KeyWord("yield", false, false);
		// soft KeyWords
		//public static readonly KeyWord UNDERSCORE = new KeyWord("_", false, false); // treat this one as a variable
		public static readonly KeyWord Case = new KeyWord("case", true, true);
		public static readonly KeyWord Match = new KeyWord("match", true, true);
		//public static readonly KeyWord NAME = new KeyWord("__name__", false, false); // treat this one as a variable

		public static char[] CharacterSet = "TFNabcdefghilmnoprstuwy_".ToCharArray();

		public static readonly KeyWord[] ALL = new KeyWord[]
		{
			And, As, Assert, Async, Await, Break, Class, Continue, Def, Del,
			Elif, Else, Except, False, Finally, For, From, Global, If, Import,
			In, Is, Lambda, None, Nonlocal, Not, Or, Pass, Raise, Return, True,
			Try, While, With, Yield, Case, Match //, UNDERSCORE, NAME
        };

		public string Value { get; set; }
		public bool IsBlockDefinition { get; set; }
		public bool IsConditionalBlock { get; set; }
		public int Length => Value.Length;
		public KeyWord(string value, bool blockDefinition, bool conditionalBlock)
		{
			Value = value;
			IsBlockDefinition = blockDefinition;
			IsConditionalBlock = conditionalBlock;
		} 

		public override bool Equals(object? other)
		{
			if (other is KeyWord kw)
			{
				if (other == null)
				{
					return false;
				}
				return kw.Value == Value;
			}

			return false;
		}

		public override string ToString()
		{
			return $"{(IsBlockDefinition ? "block " : "")}{(IsConditionalBlock ? "conditional " : "")}{Value}";
		}
	}
}

