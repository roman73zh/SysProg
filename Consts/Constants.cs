using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Consts
{
    internal class Constants
    {
        public static readonly string[] emptySymbols = { " ", "\n", "\0", "\t", "\r" };

        public static readonly string[] keywords = {
            "if",
            "else",
            "public",
            "private",
            "protected",
            "class", 
            "for", 
            "while", 
            "return", 
            "continue", 
            "break",
            "abstract",
            "internal",
            "new",
            "override",
            "readonly",
            "try",
            "catch",
            "void",
            "case",
            "const",
            "do",
            "using",
            "null",
            "switch",
            "volatile",
            "this",
            "false",
            "true",
            "foreach",
            "operator",
            "throw",
            "goto",
            "string",
            "virtual",
            "namespace"
        };

        public static readonly Dictionary<string, (int, string)> dataTypes = new Dictionary<string, (int, string)>() {
            { "int", (0, "int32") },
            { "uint", (1, "uint32") },
            { "long", (2, "int64") },
            { "ulong", (3, "uint64") },
            { "short", (4, "int16") },
            { "ushort", (5, "uint16") },
            { "byte", (6, "uint8") },
            { "sbyte", (7, "int8") },
            { "string", (8, "string")},
            { "bool", (9, "boolean")},
        };

        public static readonly string[] delimiters = { ";", ".", ",", "(", ")", "{", "}", "[", "]" };

        public static readonly Dictionary<string, (int, string, int)> operators = new Dictionary<string, (int, string, int)>() {
            {"=", (0, "assign_operation", 0)},
            {"+", (1, "sum_operation", 2)},
            {"-", (2, "sub_operation", 2)},
            {"*", (3, "mul_operation", 2)},
            {"/", (4, "div_operation", 2)},
            {"%", (5, "mod_operation", 2)},
            {"++", (6, "inc_operation", 2)},
            {"--", (7, "dec_operation", 2)},
            {"+=", (8, "add_amount_operation", 2)},
            {"-=", (9, "sub_amount_operation", 2)},
            {"*=", (10, "mul_amount_operation", 2)},
            {"/=", (11, "div_amount_operation", 2)},
            {"%=", (12, "mod_amount_operation", 2)},
            {"==", (13, "equal_comparison", 0)},
            {"!=", (14, "not_equal_comparison", 0)},
            {">", (15, "more_comparison", 2)},
            {"<", (16, "less_comparison", 2)},
            {">=", (17, "more_equ_comparison", 2)},
            {"<=", (18, "less_equ_comparison", 2)},
            {"!", (19, "inversion", 0)},
            {"&&", (20, "and_operation", 1)},
            {"||", (21, "or_operation", 1)},
            {"&", (22, "bit_and_operator", 2)},
            {"|", (23, "bit_or_operator", 2)}
        };

    }
}
