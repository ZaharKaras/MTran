﻿using Python.Core.CodeBlocks;

namespace Python.Core.Expressions
{
    public class MatchExpression : Expression
    {
        public Expression Subject { get; set; }
        public List<ConditionalCodeBlock> CaseStatements { get; set; }
    }
}
