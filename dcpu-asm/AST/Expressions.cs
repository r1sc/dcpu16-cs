using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dcpu_asm.AST
{
    abstract class Expression { }

    class LiteralValue : Expression
    {
        public string Literal;
    }

    class DecimalValue : Expression
    {
        public ushort Number;
    }

    class AddressOfExpression : Expression
    {
        public Expression Expression;
    }

    class AddExpression : Expression
    {
        public Expression Left;
        public Expression Right;
    }
}
