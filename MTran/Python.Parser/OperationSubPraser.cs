using Python.Core;
using Python.Core.Expressions;
using Python.Core.Tokens;

namespace Python.Parser
{
	public class OperationSubParser
	{
		public PythonParser Parser { get; set; }
		public OperationSubParser(PythonParser parser)
		{
			Parser = parser;
		}
		public Expression ParseDisjunction()
		{
			Expression conjunction = ParseConjunction();
			if (Parser.Peek().Value == KeyWord.Or.Value)
			{
				Parser.Advance();
				Expression other = ParseDisjunction(); // recurse to iterate through N elements
				return new EvaluatedExpression
				{
					LeftHandValue = conjunction,
					KeyWordOperator = KeyWord.Or,
					RightHandValue = other
				};
			}
			else
			{
				return conjunction;
			}
		}
		public Expression ParseConjunction()
		{
			Expression inversion = ParseInversion();
			if (Parser.Peek().Value == KeyWord.And.Value)
			{
				Parser.Advance();
				Expression other = ParseConjunction();
				return new EvaluatedExpression
				{
					LeftHandValue = inversion,
					KeyWordOperator = KeyWord.And,
					RightHandValue = other
				};
			}
			else
			{
				return inversion;
			}
		}
		public Expression ParseInversion()
		{
			if (Parser.Peek().Value == KeyWord.Not.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = null,
					KeyWordOperator = KeyWord.Not,
					RightHandValue = ParseInversion()
				};
			}
			else
			{
				return ParseComparison();
			}
		}
		public Expression ParseComparison()
		{
			Expression expression = ParseBitwiseOr();
			string next = Parser.Peek().Value;
			if (next == Operator.EqualTo.Value || next == Operator.NotEqualTo.Value ||
				next == Operator.LessThanOrEqualTo.Value || next == Operator.GreaterThanOrEqualTo.Value ||
				next == Operator.LessThan.Value || next == Operator.GreaterThan.Value ||
				next == KeyWord.Not.Value || next == KeyWord.In.Value || next == KeyWord.Is.Value)
			{
				Expression ex = ParseCompareOpBitwiseOrPair();
				(ex as EvaluatedExpression).LeftHandValue = expression;
				next = Parser.Peek().Value;
				while (next == Operator.EqualTo.Value || next == Operator.NotEqualTo.Value ||
					next == Operator.LessThanOrEqualTo.Value || next == Operator.GreaterThanOrEqualTo.Value ||
					next == Operator.LessThan.Value || next == Operator.GreaterThan.Value ||
					next == KeyWord.Not.Value || next == KeyWord.In.Value || next == KeyWord.Is.Value)
				{
					Expression lefthand = ex;
					ex = ParseCompareOpBitwiseOrPair();
					(ex as EvaluatedExpression).LeftHandValue = lefthand;
					next = Parser.Peek().Value;
				}
				return ex;
			}
			else
			{
				return expression;
			}
		}
		public Expression ParseCompareOpBitwiseOrPair()
		{
			string next = Parser.Peek().Value;
			Operator op = null;
			KeyWord kw = null;
			switch (next)
			{
				case "==":
					op = Operator.EqualTo;
					break;
				case "!=":
					op = Operator.NotEqualTo;
					break;
				case "<=":
					op = Operator.LessThanOrEqualTo;
					break;
				case ">=":
					op = Operator.GreaterThanOrEqualTo;
					break;
				case "<":
					op = Operator.LessThan;
					break;
				case ">":
					op = Operator.GreaterThan;
					break;
				case "not":
					kw = KeyWord.Not;
					break;
				case "in":
					kw = KeyWord.In;
					break;
				case "is":
					kw = KeyWord.Is;
					break;
			}
			// assumption: kw or op has a value
			Parser.Advance();
			if (kw == KeyWord.Is && Parser.Peek().Value == KeyWord.Not.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = null,
					Operator = op,
					KeyWordOperator = kw,
					RightHandValue = new EvaluatedExpression
					{
						LeftHandValue = null,
						Operator = null,
						KeyWordOperator = KeyWord.Not,
						RightHandValue = ParseBitwiseOr()
					}
				};
			}
			else
			{
				return new EvaluatedExpression
				{
					LeftHandValue = null,
					Operator = op,
					KeyWordOperator = kw,
					RightHandValue = ParseBitwiseOr()
				};
			}
		}
		public Expression ParseBitwiseOr()
		{
			Expression expression = ParseBitwiseXor();
			if (Parser.Peek().Value == Operator.BitwiseOr.Value)
			{
				Parser.Advance();
				expression = new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.BitwiseOr,
					RightHandValue = ParseBitwiseOr()
				};
			}
			return expression;
		}
		public Expression ParseBitwiseXor()
		{
			Expression expression = ParseBitwiseAnd();
			if (Parser.Peek().Value == Operator.BitwiseXor.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.BitwiseXor,
					RightHandValue = ParseBitwiseXor()
				};
			}
			return expression;
		}
		public Expression ParseBitwiseAnd()
		{
			Expression expression = ParseShiftExpr();
			if (Parser.Peek().Value == Operator.BitwiseAnd.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.BitwiseAnd,
					RightHandValue = ParseBitwiseAnd()
				};
			}
			return expression;
		}
		public Expression ParseShiftExpr()
		{
			Expression sum = ParseSum();
			Expression expression = ParsingUtils.FlipExpressionTree(sum, (op) => op == Operator.Add.Value || op == Operator.Subtract.Value);
			if (Parser.Peek().Value == Operator.LeftShift.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.LeftShift,
					RightHandValue = ParseShiftExpr()
				};
			}
			if (Parser.Peek().Value == Operator.RightShift.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.RightShift,
					RightHandValue = ParseShiftExpr()
				};
			}
			return expression;
		}
		public Expression ParseSum()
		{
			Expression expression = ParsingUtils.FlipExpressionTree(ParseTerm(), (op) => op == Operator.Multiply.Value || op == Operator.Divide.Value
																						|| op == Operator.FloorDivide.Value || op == Operator.Modulus.Value
																						|| op == "@");
			if (Parser.Peek().Value == Operator.Add.Value)
			{
				Parser.Advance();
				Expression sum = ParseSum();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.Add,
					RightHandValue = sum
				};
			}
			if (Parser.Peek().Value == Operator.Subtract.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.Subtract,
					RightHandValue = ParseSum()
				};
			}
			return expression;

		}
		public Expression ParseTerm()
		{
			Expression expression = ParsingUtils.FlipExpressionTree(ParseFactor(), (op) => op == Operator.Add.Value || op == Operator.Subtract.Value
																					|| op == Operator.BitwiseNot.Value);
			if (Parser.Peek().Value == Operator.Multiply.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.Multiply,
					RightHandValue = ParseTerm()
				};
			}
			if (Parser.Peek().Value == Operator.Divide.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.Divide,
					RightHandValue = ParseTerm()
				};
			}
			if (Parser.Peek().Value == Operator.FloorDivide.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.FloorDivide,
					RightHandValue = ParseTerm()
				};
			}
			if (Parser.Peek().Value == Operator.Modulus.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = Operator.Modulus,
					RightHandValue = ParseTerm()
				};
			}
			if (Parser.Peek().Value == "@")
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = expression,
					Operator = new Operator("@"),
					RightHandValue = ParseTerm()
				};
			}
			return expression;
		}
		public Expression ParseFactor()
		{
			if (Parser.Peek().Value == Operator.Add.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = null,
					Operator = Operator.Add,
					RightHandValue = ParseFactor()
				};
			}
			else if (Parser.Peek().Value == Operator.Subtract.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = null,
					Operator = Operator.Subtract,
					RightHandValue = ParseFactor()
				};
			}
			else if (Parser.Peek().Value == Operator.BitwiseNot.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = null,
					Operator = Operator.BitwiseNot,
					RightHandValue = ParseFactor()
				};
			}
			else
			{
				return ParsePower();
			}
		}
		public Expression ParsePower()
		{
			Expression primary = ParseAwaitPrimary();
			if (Parser.Peek().Value == Operator.Exponentiation.Value)
			{
				Parser.Advance();
				Expression factor = ParseFactor();
				return new EvaluatedExpression
				{
					LeftHandValue = primary,
					Operator = Operator.Exponentiation,
					RightHandValue = factor
				};
			}
			else
			{
				return primary;
			}
		}
		public Expression ParseAwaitPrimary()
		{
			if (Parser.Peek().Value == KeyWord.Await.Value)
			{
				Parser.Advance();
				return new EvaluatedExpression
				{
					LeftHandValue = null,
					KeyWordOperator = KeyWord.Await,
					RightHandValue = ParsingUtils.FlipExpressionTree(Parser.AtomSubParser.ParsePrimary(), c => c == Operator.ObjectReference.Value
																							|| c == "(" || c == "[")
				};
			}
			else
			{
				return ParsingUtils.FlipExpressionTree(Parser.AtomSubParser.ParsePrimary(), c => c == Operator.ObjectReference.Value
																			  || c == "(" || c == "[");
			}
		}
	}
}
