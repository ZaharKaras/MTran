﻿using Python.Core.Abstraction;

namespace Python.Lexer
{
	public abstract class Lexer
	{
		public int Position { get; private set; }
		public string Source { get; private set; }
		public int Total => Source.Length;
		public int Remaining => Total - Position;
		public Token PreviousToken { get; set; }
		public Lexer(string source)
		{
			Position = 0;
			Source = source;
		}
		public abstract Token NextToken();
		public List<Token> Consume()
		{
			List<Token> tokens = new List<Token>();
			while (Remaining > 0)
			{
				//Console.WriteLine(DateTime.Now + " : " + Position + "/" + Source.Length + " - " + Source.Substring(Position, 10));
				Token t = NextToken();
				if (t != null)
				{
					if (t.Type == TokenType.Comment)
					{
						tokens.Add(new Token
						{
							Type = TokenType.EndOfExpression,
							Value = null
						});
					}
					tokens.Add(t);
					if (t.Type != TokenType.Comment)
					{
						PreviousToken = t;
					}
				}
			}
			// make sure we end with an EoE
			tokens.Add(new Token
			{
				Type = TokenType.EndOfExpression,
				Value = "\n"
			});
			return tokens;
		}
		public char GetCurrentCharacter()
		{
			var character = Source[Position];

			return character;
		}
		public string ReadUntilWhitespace()
		{
			int start = Position, index = Position;
			while (!IsWhitespace(Source[index]))
			{
				index++;
			}
			return Source.Substring(start, index - start);
		}
		public string ReadUntilNext(char[] valid)
		{
			int start = Position, index = Position;
			while (index < Total)
			{
				char c = Source[index];
				bool isvalid = false;
				for (int idx = 0; idx < valid.Length && !isvalid; idx++)
				{
					if (valid[idx] == c)
					{
						isvalid = true;
					}
				}
				if (!isvalid)
				{
					break;
				}
				index++;
			}
			return Source.Substring(start, index - start);
		}
		public bool IsWhitespace(char c, bool tabsAsWhitespace = true)
		{
			// don't process tabs as whitespace, process them as TokenType.Tab
			return c == ' ' || /*(tabsAsWhitespace && c == '\t') ||*/ c == '\r' || c == '\n';
		}
		public void SkipWhitespace(bool tabsAsWhitespace = true, bool breakOnNewlines = true)
		{
			while (Position < Source.Length && IsWhitespace(Source[Position], tabsAsWhitespace))
			{
				if (Source[Position] == '\n' && breakOnNewlines)
				{
					break; // stop after the newline
				}
				Position++;
			}
		}
		public void SkipNext(int n)
		{
			Position += n;
		}
		public void Advance()
		{
			Position++;
		}
		public string GetNext(int n)
		{
			if (Remaining < n)
			{
				return null;
			}
			else
			{
				return Source.Substring(Position, n);
			}
		}
	}
}
