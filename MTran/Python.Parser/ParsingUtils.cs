using Python.Core;
using Python.Core.Expressions;
using Python.Core.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Python.Parser
{
	public class ParsingUtils
	{
		/// <summary>
		/// flip the tree for left-to-right evaluation
		/// </summary>
		public static Expression FlipExpressionTree(Expression expression, Func<string, bool> acceptOperator)
		{
			//
			//      /\                /\
			//       /\              /\
			//        /\     ->     /\
			//         /\          /\
			//
			List<Expression> tree = new List<Expression>();
			List<KeyWord> KeyWords = new List<KeyWord>();
			List<Operator> operators = new List<Operator>();
			Expression ex = expression;
			if (ex is EvaluatedExpression other && !acceptOperator(other.Operator?.Value ?? other.KeyWordOperator?.Value))
			{
				// If there's no chain to flip, just result
				return ex;
			}
			while (ex is EvaluatedExpression eval && acceptOperator(eval.Operator?.Value ?? eval.KeyWordOperator?.Value))
			{
				tree.Add(eval.LeftHandValue);
				KeyWords.Add(eval.KeyWordOperator);
				operators.Add(eval.Operator);
				ex = eval.RightHandValue;
			}
			tree.Add(ex);
			if (tree.Count == 1)
			{
				return tree[0];
			}
			else
			{
				Expression flipped = new EvaluatedExpression
				{
					LeftHandValue = tree[0],
					Operator = operators[0],
					KeyWordOperator = KeyWords[0],
					RightHandValue = tree[1]
				};
				for (int i = 1; i < tree.Count - 1; i++)
				{
					flipped = new EvaluatedExpression
					{
						LeftHandValue = flipped,
						Operator = operators[i],
						KeyWordOperator = KeyWords[i],
						RightHandValue = tree[i + 1]
					};
				}
				return flipped;
			}
		}
	}
}
