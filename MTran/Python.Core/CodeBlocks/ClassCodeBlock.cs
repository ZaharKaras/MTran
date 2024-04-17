using Python.Core.Abstraction;

namespace Python.Core.CodeBlocks
{
    public class ClassCodeBlock : CodeBlock
    {
        public string Name { get; set; }
        public Expression Arguments { get; set; }
        public List<Expression> Decorators { get; set; }
    }
}
