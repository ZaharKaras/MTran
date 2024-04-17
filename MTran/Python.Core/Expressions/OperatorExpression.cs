using Python.Core.Tokens;

namespace Python.Core.Expressions
{
    public class OperatorExpression : Expression
    {
        public KeyWord KeyWordOperator { get; set; }
        public Operator Operator { get; set; }
        public Expression Expression { get; set; }

        public override string ToString()
        {
            return $"{(KeyWordOperator != null ? KeyWordOperator.ToString() : Operator.ToString())} {Expression}";
        }
    }
}
