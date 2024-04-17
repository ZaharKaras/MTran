using Python.Core;
using Python.Core.Abstraction;
using Python.Core.CodeBlocks;
using Python.Core.Expressions;
using Python.Core.Tokens;

namespace Python.Parser
{
	public class CompoundSubParser
	{
		public PythonParser Parser { get; set; }
		public CompoundSubParser(PythonParser parser)
		{
			Parser = parser;
		}
		//compound_stmt:
		//    | function_def
		//    | if_stmt
		//    | class_def
		//    | with_stmt
		//    | for_stmt
		//    | try_stmt
		//    | while_stmt
		//    | match_stmt
		public Expression ParseCompoundStatement()
		{
			if (Parser.Peek().Value == "@" || (Parser.Peek().Value == KeyWord.Async.Value && Parser.Peek(1).Value == KeyWord.Def.Value)
				|| Parser.Peek().Value == KeyWord.Def.Value)
			{
				int position = Parser.Position;
				ParseDecorators();
				if (Parser.Peek().Value == KeyWord.Async.Value || Parser.Peek().Value == KeyWord.Def.Value)
				{
					//    | function_def
					return ParseFunctionDef();
				}
				else
				{
					Parser.RewindTo(position);
				}
			}
			if (Parser.Peek().Value == KeyWord.If.Value)
			{
				//    | if_stmt
				return ParseIfStmt();
			}
			if (Parser.Peek().Value == "@" || Parser.Peek().Value == KeyWord.Class.Value)
			{
				//    | class_def
				return ParseClassDef();
			}
			if (Parser.Peek().Value == KeyWord.With.Value || (Parser.Peek().Value == KeyWord.Async.Value && Parser.Peek(1).Value == KeyWord.With.Value))
			{
				//    | with_stmt
				return ParseWithStmt();
			}
			if (Parser.Peek().Value == KeyWord.For.Value || (Parser.Peek().Value == KeyWord.Async.Value && Parser.Peek(1).Value == KeyWord.For.Value))
			{
				//    | for_stmt
				return ParseForStmt();
			}
			if (Parser.Peek().Value == KeyWord.Try.Value)
			{
				//    | try_stmt
				return ParseTryStmt();
			}
			if (Parser.Peek().Value == KeyWord.While.Value)
			{
				//    | while_stmt
				return ParseWhileStmt();
			}
			if (Parser.Peek().Value == KeyWord.Match.Value)
			{
				//    | match_stmt
				return ParseMatchStmt();
			}
			return null;
		}

		//with_stmt:
		//    | 'with' '(' ','.with_item+ ','? ')' ':' block 
		//    | 'with' ','.with_item+ ':' [TYPE_COMMENT] block 
		//    | ASYNC 'with' '(' ','.with_item+ ','? ')' ':' block 
		//    | ASYNC 'with' ','.with_item+ ':' [TYPE_COMMENT]
		//        block
		public WithCodeBlock ParseWithStmt()
		{
			bool isAsynchronous = false;
			if (Parser.Peek().Value == KeyWord.Async.Value)
			{
				isAsynchronous = true;
				Parser.Advance();
			}
			Parser.Accept(KeyWord.With.Value);
			Parser.Advance();
			WithCodeBlock block = new WithCodeBlock
			{
				IsAsynchronous = isAsynchronous
			};
			if (Parser.Peek().Value == "(")
			{
				Parser.Advance();
				List<WithItem> withItems = new List<WithItem>();
				withItems.Add(ParseWithItem());
				while (Parser.Peek().Value == ",")
				{
					Parser.Advance();
					withItems.Add(ParseWithItem());
				}
				block.WithItems = withItems;
				Parser.Accept(")");
				Parser.Advance();
			}
			else
			{
				List<WithItem> withItems = new List<WithItem>();
				withItems.Add(ParseWithItem());
				while (Parser.Peek().Value == ",")
				{
					Parser.Advance();
					withItems.Add(ParseWithItem());
				}
				block.WithItems = withItems;
			}
			Parser.Accept(":");
			Parser.Advance();
			// TODO TYPE_COMMENTS?
			block.Statements = Parser.ParseBlock().Statements;
			return block;
		}
		//with_item:
		//    | expression 'as' star_target &(',' | ')' | ':') 
		//    | expression
		public WithItem ParseWithItem()
		{
			Expression item = Parser.ParseExpression();
			WithItem withItem = new WithItem
			{
				Item = item
			};
			if (Parser.Peek().Value == KeyWord.As.Value)
			{
				Parser.Advance();
				withItem.Target = Parser.AtomSubParser.ParseStarTarget();
			}
			return withItem;
		}
		//for_stmt:
		//    | 'for' star_targets 'in' ~star_expressions ':' [TYPE_COMMENT] block[else_block] 
		//    | ASYNC 'for' star_targets 'in' ~star_expressions ':' [TYPE_COMMENT] block[else_block]
		public IterableCodeBlock ParseForStmt()
		{
			bool isAsynchronous = false;
			if (Parser.Peek().Value == KeyWord.Async.Value)
			{
				isAsynchronous = true;
				Parser.Advance();
			}
			Parser.Accept(KeyWord.For.Value);
			Parser.Advance();
			var targets = Parser.AtomSubParser.ParseStarTargets();
			Parser.Accept(KeyWord.In.Value);
			Parser.Advance();
			var generators = Parser.ParseStarExpressions();
			Parser.Accept(":");
			Parser.Advance();
			// TODO TYPE_COMMENT?
			var block = Parser.ParseBlock();
			IterableCodeBlock codeBlock = new IterableCodeBlock
			{
				Targets = targets.Elements,
				Generators = generators is CollectionExpression ? (generators as CollectionExpression).Elements
															: new List<Expression>(new Expression[] { generators }),
				IsAsynchronous = isAsynchronous,
				Statements = block.Statements
			};
			if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == KeyWord.Else.Value)
			{
				var elseBlock = ParseElseStmt();
				codeBlock.ChainedCodeBlock = elseBlock;
			}
			return codeBlock;
		}
		public ConditionalCodeBlock ParseWhileStmt()
		{
			Parser.Accept(KeyWord.While.Value);
			Parser.Advance();
			Expression condition = Parser.ParseNamedExpression();
			Parser.Accept(":");
			Parser.Advance();
			CodeBlock block = Parser.ParseBlock();
			if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == KeyWord.Else.Value)
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.While,
					ChainedBlock = ParseElseStmt(),
					Statements = block.Statements
				};
			}
			else
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.While,
					Statements = block.Statements
				};
			}
		}
		public ConditionalCodeBlock ParseIfStmt()
		{
			Parser.Accept(KeyWord.If.Value);
			Parser.Advance();
			Expression condition = Parser.ParseNamedExpression();
			Parser.Accept(":");
			Parser.Advance();
			CodeBlock block = Parser.ParseBlock();
			if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == KeyWord.Elif.Value)
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.If,
					ChainedBlock = ParseElifStmt(),
					Statements = block.Statements
				};
			}
			else if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == KeyWord.Else.Value)
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.If,
					ChainedBlock = ParseElseStmt(),
					Statements = block.Statements
				};
			}
			else
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.If,
					Statements = block.Statements
				};
			}
		}
		public ConditionalCodeBlock ParseElifStmt()
		{
			Parser.Accept(KeyWord.Elif.Value);
			Parser.Advance();
			Expression condition = Parser.ParseNamedExpression();
			Parser.Accept(":");
			Parser.Advance();
			CodeBlock block = Parser.ParseBlock();
			if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == KeyWord.Elif.Value)
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.Elif,
					ChainedBlock = ParseElifStmt(),
					Statements = block.Statements
				};
			}
			else if (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == KeyWord.Else.Value)
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.Elif,
					ChainedBlock = ParseElseStmt(),
					Statements = block.Statements
				};
			}
			else
			{
				return new ConditionalCodeBlock
				{
					Condition = condition,
					Type = ConditionalType.Elif,
					Statements = block.Statements
				};
			}
		}
		public ConditionalCodeBlock ParseElseStmt()
		{
			Parser.Accept(KeyWord.Else.Value);
			Parser.Advance();
			Parser.Accept(":");
			Parser.Advance();
			CodeBlock block = Parser.ParseBlock();
			return new ConditionalCodeBlock
			{
				Type = ConditionalType.Else,
				Statements = block.Statements
			};
		}

		public MatchExpression ParseMatchStmt()
		{
			Parser.Accept(KeyWord.Match.Value);
			Parser.Advance();
			Expression subject = ParseSubjectExpr();
			Parser.Accept(":");
			Parser.Advance();
			Parser.Accept("\n");
			Parser.Advance();
			Parser.Accept(TokenType.IndentTab);
			Parser.Advance();
			List<ConditionalCodeBlock> cases = new List<ConditionalCodeBlock>();
			while (Parser.Position < Parser.Tokens.Count && Parser.Peek().Value == KeyWord.Case.Value)
			{
				cases.Add(ParseCaseStatement());
			}
			if (Parser.Position < Parser.Tokens.Count)
			{
				Parser.Accept(TokenType.DedentTab);
				Parser.Advance();
			}
			return new MatchExpression
			{
				Subject = subject,
				CaseStatements = cases
			};
		}

		//subject_expr:
		//    | ','.star_named_expression+ [','] 
		//    | named_expression
		public Expression ParseSubjectExpr()
		{
			// not sure when a star_named_expression wouldn't match for a named_expression...
			Expression e1 = Parser.ParseStarNamedExpression();
			if (Parser.Peek().Value == ",")
			{
				CollectionExpression collection = new CollectionExpression
				{
					Type = CollectionType.List
				};
				collection.Elements.Add(e1);
				while (Parser.Peek().Value == ",")
				{
					Parser.Advance();
					collection.Elements.Add(Parser.ParseStarNamedExpression());
				}
				return collection;
			}
			else
			{
				return e1;
			}
		}

		public ConditionalCodeBlock ParseCaseStatement()
		{
			Parser.Accept(KeyWord.Case.Value);
			Parser.Advance();
			Expression pattern = ParsePatternsAndGuard();
			Parser.Accept(":");
			Parser.Advance();
			CodeBlock block = Parser.ParseBlock();
			return new ConditionalCodeBlock
			{
				Type = ConditionalType.Case,
				Condition = pattern,
				Statements = block.Statements
			};
		}
		public Expression ParsePatternsAndGuard()
		{
			PatternExpression pattern = Parser.PatternSubParser.ParsePatterns();
			if (Parser.Peek().Value == KeyWord.If.Value)
			{
				Parser.Advance();
				pattern.Guard = Parser.ParseNamedExpression();
			}
			return pattern;
		}
		//function_def:
		//   | decorators function_def_raw 
		//   | function_def_raw
		public FunctionCodeBlock ParseFunctionDef()
		{
			List<Expression> decorators = ParseDecorators();
			FunctionCodeBlock block = ParseFunctionDefRaw();
			block.Decorators = decorators;
			return block;
		}
		//function_def_raw:
		//    | 'def' NAME '(' [params] ')' ['->' expression] ':' [func_type_comment] block 
		//    | ASYNC 'def' NAME '(' [params] ')' ['->' expression] ':' [func_type_comment] block
		public FunctionCodeBlock ParseFunctionDefRaw()
		{
			bool isAsync = false;
			if (Parser.Peek().Value == KeyWord.Async.Value)
			{
				Parser.Advance();
				isAsync = true;
			}
			Parser.Accept(KeyWord.Def.Value);
			Parser.Advance();
			string name = Parser.Peek().Value;
			Parser.Advance();
			Parser.Accept("(");
			Parser.Advance();
			FunctionCodeBlock block = new FunctionCodeBlock
			{
				Name = name,
				IsAsynchronous = isAsync
			};
			if (Parser.Peek().Value != ")")
			{
				block.Parameters = ParseParams();
			}
			Parser.Accept(")");
			Parser.Advance();
			if (Parser.Peek().Value == "->" || Parser.Peek().Type == TokenType.ReturnHint)
			{
				Parser.Advance();
				block.ReturnHint = Parser.ParseExpression();
			}
			Parser.Accept(":");
			Parser.Advance();
			block.Statements = Parser.ParseBlock().Statements;
			return block;
		}
		//func_type_comment:
		//    | NEWLINE TYPE_COMMENT &(NEWLINE INDENT)   # Must be followed by indented block
		//    | TYPE_COMMENT
		// decorators: ('@' named_expression NEWLINE )+
		public List<Expression> ParseDecorators()
		{
			List<Expression> values = new List<Expression>();
			while (Parser.Peek().Value == "@")
			{
				Parser.Advance();
				values.Add(Parser.ParseNamedExpression());
				Parser.Accept("\n");
				Parser.Advance();
			}
			return values;
		}
		//params:
		//    | parameters
		public List<ParameterExpression> ParseParams()
		{
			return ParseParameters();
		}

		//parameters:
		//    | slash_no_default param_no_default* param_with_default*[star_etc] 
		//    | slash_with_default param_with_default* [star_etc] 
		//    | param_no_default+ param_with_default*[star_etc] 
		//    | param_with_default+ [star_etc] 
		//    | star_etc
		public List<ParameterExpression> ParseParameters()
		{
			int previous = Parser.Position;
			//try
			{
				List<ParameterExpression> parameters = ParseSlashNoDefault();
				while (Parser.Position + 1 < Parser.Tokens.Count && Parser.Peek(1).Value != "=" && Parser.Peek().Value != ")" && Parser.Peek().Value != "*")
				{
					parameters.Add(ParseParamNoDefault());
				}
				while (Parser.Position + 1 < Parser.Tokens.Count && Parser.Peek(1).Value == "=" && Parser.Peek().Value != ")" && Parser.Peek().Value != "*")
				{
					parameters.Add(ParseParamWithDefault());
				}
				if (Parser.Peek().Value == "**" || Parser.Peek().Value == "*")
				{
					parameters.AddRange(ParseStarEtc());
				}
				if (!Parser.HasErrors())
				{
					return parameters;
				}
			}
			//catch (Exception)
			{
				Parser.RewindTo(previous);
			}
			//try
			{
				List<ParameterExpression> parameters = ParseSlashWithDefault();
				while (Parser.Position + 1 < Parser.Tokens.Count && Parser.Peek(1).Value == "=" && Parser.Peek().Value != ")" && Parser.Peek().Value != "*")
				{
					parameters.Add(ParseParamWithDefault());
				}
				if (Parser.Peek().Value == "**" || Parser.Peek().Value == "*")
				{
					parameters.AddRange(ParseStarEtc());
				}
				if (!Parser.HasErrors())
				{
					return parameters;
				}
			}
			//catch (Exception)
			{
				Parser.RewindTo(previous);
			}
			if (Parser.Peek().Value != "**" && Parser.Peek().Value != "*")
			{
				List<ParameterExpression> parameters = new List<ParameterExpression>();
				if (Parser.Peek(1).Value != "=")
				{
					while (Parser.Peek(1).Value != "=" && Parser.Peek().Value != ")" && Parser.Peek().Value != "*")
					{
						parameters.Add(ParseParamNoDefault());
					}
				}
				while (Parser.Peek(1).Value == "=" && Parser.Peek().Value != ")" && Parser.Peek().Value != "*")
				{
					parameters.Add(ParseParamWithDefault());
				}
				if (Parser.Peek().Value == "**" || Parser.Peek().Value == "*")
				{
					parameters.AddRange(ParseStarEtc());
				}
				return parameters;
			}
			else
			{
				List<ParameterExpression> parameters = new List<ParameterExpression>();
				if (Parser.Peek().Value == "**" || Parser.Peek().Value == "*")
				{
					parameters.AddRange(ParseStarEtc());
				}
				return parameters;
			}
		}

		//slash_no_default:
		//    | param_no_default+ '/' ',' 
		//    | param_no_default+ '/' &')'
		public List<ParameterExpression> ParseSlashNoDefault()
		{
			List<ParameterExpression> parameters = new List<ParameterExpression>();
			while (Parser.Peek().Value != "/" && Parser.Peek().Value != ")")
			{
				parameters.Add(ParseParamNoDefault());
			}
			Parser.Accept("/");
			Parser.Advance();
			if (Parser.Peek().Value == ",")
			{
				Parser.Advance();
			}
			return parameters;
		}
		//slash_with_default:
		//    | param_no_default* param_with_default+ '/' ',' 
		//    | param_no_default* param_with_default+ '/' &')' 
		public List<ParameterExpression> ParseSlashWithDefault()
		{
			List<ParameterExpression> parameters = new List<ParameterExpression>();
			while (Parser.Peek(1).Value != "=" && Parser.Peek().Value != ")")
			{
				parameters.Add(ParseParamNoDefault());
			}
			while (Parser.Peek(1).Value == "=" && Parser.Peek().Value != ")")
			{
				parameters.Add(ParseParamWithDefault());
			}
			Parser.Accept("/");
			Parser.Advance();
			if (Parser.Peek().Value == ",")
			{
				Parser.Advance();
			}
			return parameters;
		}
		//star_etc:
		//    | '*' param_no_default param_maybe_default* [kwds] 
		//    | '*' ',' param_maybe_default+ [kwds]
		// PEP 3102
		//    | kwds
		public List<ParameterExpression> ParseStarEtc()
		{
			if (Parser.Peek().Value == "**")
			{
				return new List<ParameterExpression>(new ParameterExpression[] { ParseKwds() });
			}
			Parser.Accept("*");
			Parser.Advance();
			if (Parser.Peek().Value == ",")
			{
				Parser.Advance();
				List<ParameterExpression> parameters = new List<ParameterExpression>();
				parameters.Add(ParseParamMaybeDefault());
				while (Parser.Peek().Value != "**" && Parser.Peek().Value != ")")
				{
					parameters.Add(ParseParamMaybeDefault());
				}
				if (Parser.Peek().Value == "**")
				{
					parameters.Add(ParseKwds());
				}
				foreach (ParameterExpression ex in parameters)
				{
					ex.KeyWordOnly = true;
				}
				return parameters;
			}
			else
			{
				List<ParameterExpression> parameters = new List<ParameterExpression>();
				parameters.Add(ParseParamNoDefault());
				parameters[0].ListGenerator = true;
				while (Parser.Peek().Value != "**" && Parser.Peek().Value != ")")
				{
					parameters.Add(ParseParamMaybeDefault());
				}
				if (Parser.Peek().Value == "**")
				{
					parameters.Add(ParseKwds());
				}
				return parameters;
			}
		}
		//kwds: '**' param_no_default
		public ParameterExpression ParseKwds()
		{
			Parser.Accept("**");
			Parser.Advance();
			ParameterExpression param = ParseParamNoDefault();
			param.DictionaryGenerator = true;
			return param;
		}
		//param_no_default:
		//    | param ',' TYPE_COMMENT? 
		//    | param TYPE_COMMENT? &')'
		public ParameterExpression ParseParamNoDefault()
		{
			// TODO TYPE_COMMENT?
			ParameterExpression param = ParseParam();
			if (Parser.Peek().Value == ",")
			{
				Parser.Advance();
			}
			return param;
		}
		//param_with_default:
		//    | param default ',' TYPE_COMMENT? 
		//    | param default TYPE_COMMENT? &')'
		public ParameterExpression ParseParamWithDefault()
		{
			// TODO TYPE_COMMENT?
			ParameterExpression param = ParseParam();
			Parser.Accept("=");
			if (Parser.Peek().Value == "=")
			{
				param.Default = ParseDefault();
			}
			if (Parser.Peek().Value == ",")
			{
				Parser.Advance();
			}
			return param;
		}
		//param_maybe_default:
		//    | param default? ',' TYPE_COMMENT? 
		//    | param default? TYPE_COMMENT? &')'
		public ParameterExpression ParseParamMaybeDefault()
		{
			// TODO TYPE_COMMENT?
			ParameterExpression param = ParseParam();
			if (Parser.Peek().Value == "=")
			{
				param.Default = ParseDefault();
			}
			if (Parser.Peek().Value == ",")
			{
				Parser.Advance();
			}
			return param;
		}
		//param: NAME annotation?
		public ParameterExpression ParseParam()
		{
			string name = Parser.Peek().Value;
			Parser.Advance();
			ParameterExpression param = new ParameterExpression
			{
				Name = name
			};
			if (Parser.Peek().Value == ":")
			{
				param.Annotation = ParseAnnotation();
			}
			return param;
		}
		//annotation: ':' expression
		public Expression ParseAnnotation()
		{
			Parser.Accept(":");
			Parser.Advance();
			return Parser.ParseExpression();
		}
		//default: '=' expression
		public Expression ParseDefault()
		{
			Parser.Accept("=");
			Parser.Advance();
			return Parser.ParseExpression();
		}
		//class_def:
		//    | decorators class_def_raw 
		//    | class_def_raw
		public ClassCodeBlock ParseClassDef()
		{
			List<Expression> decorators = null;
			if (Parser.Peek().Value == "@")
			{
				decorators = ParseDecorators();
			}
			ClassCodeBlock block = ParseClassDefRaw();
			block.Decorators = decorators;
			return block;
		}
		//class_def_raw:
		//    | 'class' NAME['('[arguments] ')'] ':' block
		public ClassCodeBlock ParseClassDefRaw()
		{
			Parser.Accept(KeyWord.Class.Value);
			Parser.Advance();
			string name = Parser.Peek().Value;
			Parser.Advance();
			ClassCodeBlock block = new ClassCodeBlock
			{
				Name = name
			};
			if (Parser.Peek().Value == "(")
			{
				Parser.Advance();
				block.Arguments = Parser.ArgumentsSubParser.ParseArguments();
				Parser.Accept(")");
				Parser.Advance();
			}
			Parser.Accept(":");
			Parser.Advance();
			block.Statements = Parser.ParseBlock().Statements;
			return block;
		}
		//try_stmt:
		//    | 'try' ':' block finally_block 
		//    | 'try' ':' block except_block+ [else_block] [finally_block]
		public TryCatchCodeBlock ParseTryStmt()
		{
			Parser.Accept(KeyWord.Try.Value);
			Parser.Advance();
			Parser.Accept(":");
			Parser.Advance();
			CodeBlock block = Parser.ParseBlock();
			if (Parser.Peek().Value == KeyWord.Except.Value)
			{
				List<CatchCodeBlock> catchBlocks = new List<CatchCodeBlock>();
				while (Parser.Peek().Value == KeyWord.Except.Value)
				{
					catchBlocks.Add(ParseExceptBlock());
				}
				TryCatchCodeBlock tryBlock = new TryCatchCodeBlock
				{
					Statements = block.Statements,
					CatchBlocks = catchBlocks
				};
				if (Parser.Peek().Value == KeyWord.Else.Value)
				{
					Parser.Advance();
					Parser.Accept(":");
					Parser.Advance();
					tryBlock.ElseBlock = Parser.ParseBlock();
				}
				if (Parser.Peek().Value == KeyWord.Finally.Value)
				{
					Parser.Advance();
					Parser.Accept(":");
					Parser.Advance();
					tryBlock.FinallyBlock = Parser.ParseBlock();
				}
				return tryBlock;
			}
			else
			{
				CodeBlock finallyBlock = ParseFinallyBlock();
				return new TryCatchCodeBlock
				{
					Statements = block.Statements,
					FinallyBlock = finallyBlock
				};
			}
		}
		//except_block:
		//    | 'except' expression['as' NAME] ':' block 
		//    | 'except' ':' block
		public CatchCodeBlock ParseExceptBlock()
		{
			Parser.Accept(KeyWord.Except.Value);
			Parser.Advance();
			if (Parser.Peek().Value == ":")
			{
				CodeBlock block = Parser.ParseBlock();
				Parser.Advance();
				return new CatchCodeBlock
				{
					Statements = block.Statements
				};
			}
			else
			{
				Expression capture = Parser.ParseExpression();
				string alias = null;
				if (Parser.Peek().Value == KeyWord.As.Value)
				{
					Parser.Advance();
					alias = Parser.Peek().Value;
					Parser.Advance();
				}
				Parser.Accept(":");
				Parser.Advance();
				CodeBlock block = Parser.ParseBlock();
				return new CatchCodeBlock
				{
					Capture = capture,
					Alias = alias,
					Statements = block.Statements
				};
			}
		}
		//finally_block:
		//    | 'finally' ':' block
		public CodeBlock ParseFinallyBlock()
		{
			Parser.Accept(KeyWord.Finally.Value);
			Parser.Advance();
			return Parser.ParseBlock();
		}
	}
}
