using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Lifetime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using dcpu_asm.AST;
using Sprache;

namespace dcpu_asm
{
    class Program
    {
        private static List<string> binaryInstructions = new List<string>
        {
            "SET","ADD","SUB","MUL","MLI","DIV","DVI","MOD","MDI","AND","BOR","XOR","SHR","ASR","SHL","IFB","IFC","IFE","IFN","IFG","IFA","IFL","IFU","ADX","SBX","STI","STD"
        };
        private static List<string> unaryInstructions = new List<string>
        {
            "JSR","INT","IAG","IAS","RFI","IAQ","HWN","HWQ","HWI"
        };

        private static CommentParser Comment = new CommentParser(";", null, null, "\n");
        
        private static Parser<LiteralValue> LiteralValueParser =
            from start in Parse.Letter
            from rest in Parse.LetterOrDigit.Many().Text()
            select new LiteralValue {Literal = start + rest};

        private static Parser<ushort> NumberParser =
            from digits in Parse.Digit.AtLeastOnce().Text()
            select ushort.Parse(digits);

        private static Parser<ushort> HexParser =
            from leading in Parse.String("0x")
            from rest in Parse.LetterOrDigit.AtLeastOnce().Text()
            select ushort.Parse(rest, NumberStyles.AllowHexSpecifier);

        private static Parser<DecimalValue> DecimalValueParser =
            from number in HexParser.Or(NumberParser)
            select new DecimalValue {Number = number};

        private static Parser<Expression> ValueExpressionParser = LiteralValueParser.XOr<Expression>(DecimalValueParser);

        private static Parser<AddExpression> AddExpressionParser =
            from left in ValueExpressionParser
            from plus in Parse.Char('+')
            from right in ValueExpressionParser
            select new AddExpression
            {
                Left = left,
                Right = right
            };

        private static Parser<AddressOfExpression> AddressOfExpressionParser =
            from lbracket in Parse.Char('[')
            from expression in AddExpressionParser.Or(ValueExpressionParser)
            from rbracket in Parse.Char(']')
            select new AddressOfExpression {Expression = expression};

        private static Parser<Expression> ExpressionParser =
            AddressOfExpressionParser.XOr<Expression>(AddExpressionParser).Or(ValueExpressionParser);

        private static Parser<string> MnemonicParser = Parse.Letter.Repeat(3).Text();

        private static Parser<InstructionStatement> BinaryInstructionStatementParser =
            from mnemonic in MnemonicParser.Where(x => binaryInstructions.Contains(x.ToUpper()))
            from whitespace in Parse.WhiteSpace.AtLeastOnce()
            from b in ExpressionParser
            from comma in Parse.Char(',').Token()
            from a in ExpressionParser
            select new BinaryInstructionStatement
            {
                Mnemonic = mnemonic,
                A = a,
                B = b
            };

        private static Parser<InstructionStatement> UnaryInstructionStatementParser =
            from mnemonic in MnemonicParser.Where(x => unaryInstructions.Contains(x.ToUpper()))
            from whitespace in Parse.WhiteSpace.AtLeastOnce()
            from a in ExpressionParser
            select new UnaryInstructionStatement
            {
                Mnemonic = mnemonic,
                A = a
            };

        private static Parser<DataExpression> DataStringParser =
            from leading in Parse.Char('"')
            from text in Parse.CharExcept("\"\n").Many().Text()
            from trailing in Parse.Char('"')
            select new DataString {Text = text};

        private static Parser<DataExpression> DataValueParser =
            from value in HexParser.Or(NumberParser)
            select new DataValue { Value = value };


        private static Parser<DataStatement> DataStatementParser =
            from dat in Parse.IgnoreCase("dat").Text()
            from whitespace in Parse.WhiteSpace.AtLeastOnce()
            from data in DataStringParser.XOr(DataValueParser).Token().DelimitedBy(Parse.Char(','))
            select new DataStatement {Expressions = data};

        private static Parser<LabelStatement> LabelParser =
            from first in Parse.Char(':')
            from name in Parse.LetterOrDigit.AtLeastOnce().Text()
            from end in Parse.WhiteSpace.Many().Or(Parse.LineEnd)
            select new LabelStatement { Name = name};

        private static Parser<Statement> StatementParser =
            from statement in
                DataStatementParser.XOr<Statement>(BinaryInstructionStatementParser.Or(UnaryInstructionStatementParser))
            select statement;

        private static Parser<Statement> LineParser =
            LabelParser.Or(StatementParser);

        private static Parser<IEnumerable<Statement>> ProgramParser = LineParser.Many().End();

        //private static Parser<AddressOfExpression> AddressOfParser =
        //    from lbracket in Parse.Char('[')
        //    from 


        private static string asm = @"
; Assembler test for DCPU
; by Markus Persson

             set a, 0xbeef                        ; Assign 0xbeef to register a
             set [0x1000], a                      ; Assign memory at 0x1000 to value of register a
             ifn a, [0x1000]                      ; Compare value of register a to memory at 0x1000 ..
                 set PC, end                      ; .. and jump to end if they don't match

             set i, 0                             ; Init loop counter, for clarity
:nextchar    ife [data+i], 0                      ; If the character is 0 ..
                 set PC, end                      ; .. jump to the end
             set [0x8000+i], [data+i]             ; Video ram starts at 0x8000, copy char there
             add i, 1                             ; Increase loop counter
             set PC, nextchar                     ; Loop
  
:data        dat ""Hello world!"", 0                ; Zero terminated string

:end         sub PC, 1                            ; Freeze the CPU forever
";

        static void Main(string[] args)
        {
            //var res = ProgramParser.Parse(asm.ToUpper());
            var a = ValueExpressionParser.Parse("abc2");
            var b = ValueExpressionParser.Parse("2345abc");
            var c = ValueExpressionParser.Parse("0x100");
            var d = AddExpressionParser.Parse("0x100+a");
            var e = AddExpressionParser.Parse("a+0x100");
            var f = AddExpressionParser.Parse("a+12");
            var g = AddExpressionParser.Parse("102+a");
            var h = AddressOfExpressionParser.Parse("[102+a]");
            var i = BinaryInstructionStatementParser.Parse("set a, 0xbeef");
            var j = BinaryInstructionStatementParser.Parse("ife [data+i], 0");
            var k = BinaryInstructionStatementParser.Parse("set [0x8000+i], [data+i]  ");
            var l = DataStatementParser.Parse("dat \"Hello world!\", 0");
            var m = LabelParser.Parse(":data");
            var n = LineParser.Parse(":nextchar ife [data+i], 0  ");
            
        }
    }
}
