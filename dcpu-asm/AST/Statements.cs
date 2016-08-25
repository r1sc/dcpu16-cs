using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcpu_asm.AST
{
    abstract class Statement { }
    
    class LabelStatement : Statement
    {
        public string Name;
    }
    
    abstract class InstructionStatement : Statement
    {
        public string Mnemonic;
    }

    class UnaryInstructionStatement : InstructionStatement
    {
        public Expression A;
    }

    class BinaryInstructionStatement : InstructionStatement
    {
        public Expression A;
        public Expression B;
    }

    abstract class DataExpression { }

    class DataString : DataExpression
    {
        public string Text;
    }

    class DataValue : DataExpression
    {
        public ushort Value;
    }

    class DataStatement : Statement
    {
        public IEnumerable<DataExpression> Expressions = new List<DataExpression>();
    }
}
