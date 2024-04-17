﻿using Python.Core.Abstraction;

namespace Python.Parser
{
	public class SyntaxError
	{
		public int Position { get; set; }
	}
	public abstract class Parser
	{
		public List<Token> Tokens { get; set; }
		public int Position { get; private set; }
		public List<SyntaxError> Errors { get; set; }
		public Parser(List<Token> tokens)
		{
			Tokens = tokens.Where(t => t.Type != TokenType.Comment && t.Type != TokenType.Tab).ToList();
			// strip comments before parsing, and skip tabs that aren't indent/dedent
			Position = 0;
			Errors = new List<SyntaxError>();
		}
		public void RewindTo(int position)
		{
			Position = position;
		}
		public void Advance(int n = 1)
		{
			Position += n;
		}
		public void Accept(TokenType type)
		{
			if (Peek().Type != type)
			{
				ThrowSyntaxError(Position);
			}
		}
		public void Accept(string value)
		{
			if (Peek().Value != value)
			{
				ThrowSyntaxError(Position);
			}
		}
		public void DontAccept(string value)
		{
			if (Peek().Value == value)
			{
				ThrowSyntaxError(Position);
			}
		}
		/*
        public int FindNext(TokenType type, int position = -1)
        {
            int pos = position >= 0 ? position : Position;
            while (pos < Tokens.Count && Tokens[pos].Type != type) // stop when we get to the token or the EOF
            {
                pos++;
            }
            return pos;
        }
        public bool Contains(string value, int start, int end)
        {
            int pos = start;
            while (pos < end)
            {
                if (Tokens[pos].Value == value)
                {
                    return true;
                }
                pos++;
            }
            return false;
        }
        public int FindEndOfRegion(TokenType nestedType, TokenType endOfNestedType, int start)
        {
            int end = start + 1, count = 1;
            while (count > 0)
            {
                if (Tokens[end].Type == nestedType)
                {
                    count++;
                }
                if (Tokens[end].Type == endOfNestedType)
                {
                    count--;
                }
                end++;
            }
            return end;
        }
        */
		public bool HasErrors(bool clear = true)
		{
			int count = Errors.Count;
			if (clear)
			{
				Errors.Clear();
			}
			return count > 0;
		}
		public void ThrowSyntaxError(int position)
		{
			//throw new Exception("Syntax error! '" + Tokens[position].Value + "'");
			Errors.Add(new SyntaxError
			{
				Position = position
			});
		}
		public Token Peek(int n = 0)
		{
			try
			{
				return Tokens[Position + n];
			}
			catch (Exception)
			{
				throw new Exception("Syntax error!");
			}
		}
		public int IndexOf(string value)
		{
			int index = -1, search = Position;
			while (search < Tokens.Count)
			{
				if (Tokens[search].Value == value)
				{
					index = search;
					break;
				}
				search++;
			}
			return index;
		}
		public int IndexOf(TokenType type)
		{
			int index = -1, search = Position;
			while (search < Tokens.Count)
			{
				if (Tokens[search].Type == type)
				{
					index = search;
					break;
				}
				search++;
			}
			return index;
		}
	}
}
