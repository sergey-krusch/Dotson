using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dotson.Reading
{
    [TestClass]
    public class LexerTests
    {
        private static readonly Token TDS = T(TokenType.DictionaryStart, null);
        private static readonly Token TDE = T(TokenType.DictionaryEnd, null);
        private static readonly Token TCL = T(TokenType.Colon, null);
        private static readonly Token TCM = T(TokenType.Comma, null);
        private static readonly Token TAS = T(TokenType.ArrayStart, null);
        private static readonly Token TAE = T(TokenType.ArrayEnd, null);

        private static Token T(TokenType @type, string value)
        {
            return new Token(@type, value);
        }

        private static Token TSQ(string s)
        {
            return T(TokenType.String, '"' + s + '"');
        }

        private static Token TN(string s)
        {
            return T(TokenType.Number, s);
        }

        private static Token TL(string s)
        {
            return T(TokenType.Literal, s);
        }

        [TestMethod]
        public void AcceptCorrectLiterals()
        {
            var input = new[] { "true", "false", "none" };
            foreach (var i in input)
            {
                var l = new Lexer(new StringReader(i));
                Assert.AreEqual(TokenType.Literal, l.GetCurrentToken().TokenType);
                Assert.AreEqual(i, l.GetCurrentToken().Value);
            }
        }

        [TestMethod]
        public void ThrowIncorrectLiterals()
        {
            var input = new[] { "True", "Yes", "yes", "False", "No", "no", "None", "Null", "null", "abc" };
            foreach (var i in input)
            {
                Exception ee = null;
                try
                {
                    var l = new Lexer(new StringReader(i));
                    l.GetCurrentToken();
                }
                catch (Exception e)
                {
                    ee = e;
                }
                Assert.IsNotNull(ee);
            }
        }

        [TestMethod]
        public void AcceptCorrectNumbers()
        {
            foreach (var i in EnumerateCorrectNumbers())
            {
                var l = new Lexer(new StringReader(i));
                Assert.AreEqual(TokenType.Number, l.GetCurrentToken().TokenType);
                if (i != l.GetCurrentToken().Value)
                {
                    l = new Lexer(new StringReader(i));
                    l.GetCurrentToken();
                }
                Assert.AreEqual(i, l.GetCurrentToken().Value);
            }
        }

        [TestMethod]
        public void ThrowIncorrectNumbers()
        {
            var input = new[] {
                "a", "%10",
                "+", "+1", "+a",
                "-", "--", "-.5", "-e2", "-E2", "-a",
                ".", ".7", ".+", ".-", "e5", "E5", ".e5", ".E5", ".a", "ae5",
                "0.e4", "0.E4", "0.", "0.+", "0.-", "0.a",
                "0e", "0E", "0e+", "0E-", "0ec", "0Ec",
                "0.1e", "0.1E", "0.1e+", "0.1e-", "0.1ea"
            };
            foreach (var i in input)
            {
                Exception ee = null;
                try
                {
                    var l = new Lexer(new StringReader(i));
                    l.GetCurrentToken();
                }
                catch (Exception e)
                {
                    ee = e;
                }
                Assert.IsNotNull(ee);
            }
        }

        [TestMethod]
        public void AcceptCorrectStrings()
        {
            foreach (var i in EnumerateCorrectStrings())
            {
                var l = new Lexer(new StringReader(i));
                Assert.AreEqual(TokenType.String, l.GetCurrentToken().TokenType);
                if (i != l.GetCurrentToken().Value)
                {
                    l = new Lexer(new StringReader(i));
                    l.GetCurrentToken();
                }
                Assert.AreEqual(i, l.GetCurrentToken().Value);
            }
        }

        [TestMethod]
        public void ThrowIncorrectStrings()
        {
            var input = new List<string>();
            input.AddRange(new[] { @"""", @"""\""", @"""\u0022", @"""abc" });
            input.AddRange(Quote(@"\B", @"\F", @"\N", @"\R", @"\T", @"\a", @"\#"));
            input.AddRange(Quote(@"\U0012", @"\u0", @"\u01", @"\u012", @"\u000G", @"\u000g"));
            input.AddRange(Quote("\u0011\u0000"));
            foreach (var i in input)
            {
                Exception ee = null;
                try
                {
                    var l = new Lexer(new StringReader(i));
                    l.GetCurrentToken();
                }
                catch (Exception e)
                {
                    ee = e;
                }
                Assert.IsNotNull(ee);
            }
        }

        [TestMethod]
        public void IgnoreWhitespaces()
        {
            var input = new[]
            {
                Case("\x09\x20\x0A\x0D"),
                Case("\x20\x09\x0A\x0D{}", TDS, TDE),
                Case("[\x20\x0A\x09\x0D]", TAS, TAE),
                Case(":\x20\x0A\x0D\x09", TCL),
                Case(" false  true\x0A\x20\x0D\x09none  ", TL("false"), TL("true"), TL("none"))
            };
            foreach (var c in input)
                CheckLexerOutput(c.Item2, new Lexer(new StringReader(c.Item1)));
        }

        [TestMethod]
        public void AcceptStructuralCharacters()
        {
            var input = new[]
            {
                Case("{}[]:,", TDS, TDE, TAS, TAE, TCL, TCM),
                Case("{{}}", TDS, TDS, TDE, TDE),
                Case("{{0.7}}", TDS, TDS, TN("0.7"), TDE, TDE),
                Case("{{:}}", TDS, TDS, TCL, TDE, TDE),
                Case("[[],[]]", TAS, TAS, TAE, TCM, TAS, TAE, TAE),
                Case(@"{""a"":[],""b"":{}}", TDS, TSQ("a"), TCL, TAS, TAE, TCM, TSQ("b"), TCL, TDS, TDE, TDE),
            };
            foreach (var c in input)
                CheckLexerOutput(c.Item2, new Lexer(new StringReader(c.Item1)));
        }

        private IEnumerable<string> EnumerateCorrectNumbers()
        {
            var startSignVariants = new[] { "", "-" };
            var floorVariants = new[] {
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "10", "11", "17", "28",
                "100", "101", "110", "111", "293", "795",
                "800000000000000000000000000000000000000000000"
            };
            var fractionVariants = new[] {
                "",
                ".0", ".1", ".2", ".3", ".4", ".5", ".6", ".7", ".8", ".9",
                ".00", ".000", ".00000", ".30", ".300", ".30000",
                ".11", ".17", ".28", ".73",
                ".389", ".807", ".999",
                ".38000", ".0000000000000000000001111", ".11111110000000000000000",
                ".0000000000000000000000000000000000000000000000000"
            };
            var expPrefixVariants = new[] {
                "e", "E",
                "e+", "E+",
                "e-", "E-"
            };
            var expSuffixVariants = new[] {
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "00", "000", "00000", "30", "300", "30000",
                "11", "17", "28", "73",
                "389", "807", "999",
                "38000", "0000000000000000000001111", "11111110000000000000000",
                "0000000000000000000000000000000000000000000000000"
            };
            var sb = new StringBuilder();
            foreach (var startSign in startSignVariants)
            {
                sb.Append(startSign);
                foreach (var floor in floorVariants)
                {
                    sb.Append(floor);
                    foreach (var fraction in fractionVariants)
                    {
                        sb.Append(fraction);
                        yield return sb.ToString();
                        foreach (var expPrefix in expPrefixVariants)
                        {
                            sb.Append(expPrefix);
                            foreach (var expSuffix in expSuffixVariants)
                            {
                                sb.Append(expSuffix);
                                yield return sb.ToString();
                                Pop(sb, expSuffix);
                            }
                            Pop(sb, expPrefix);
                        }
                        Pop(sb, fraction);
                    }
                    Pop(sb, floor);
                }
                Pop(sb, startSign);
            }
        }

        private IEnumerable<string> EnumerateCorrectStrings()
        {
            yield return @"""""";
            var charVariants = new[] {
                @"\""", @"\\", @"\/",
                @"\b", @"\f", @"\n", @"\r", @"\t",
                @"\u0000", @"\u1234", @"\u5678", @"\u9012",
                @"\uabcd", @"\u09ef",
                @"\uABCD", @"\uEF75",
                "\x20", "\x21", "\x23", "\x40", "\x5B", "\x5D", "\u1000", "\uF123", "\U000AEF05", "\U0010FFFF"
            };
            var sb = new StringBuilder(@"""");
            var l = charVariants.Length;
            for (int i = 0; i < l; ++i)
            {
                sb.Append(charVariants[i]);
                for (int j = 0; j < l; ++j)
                {
                    sb.Append(charVariants[j]);
                    for (int k = 0; k < l; ++k)
                    {
                        sb.Append(charVariants[k]);
                        sb.Append('"');
                        yield return sb.ToString();
                        sb.Remove(sb.Length - 1, 1);
                        Pop(sb, charVariants[k]);
                    }
                    Pop(sb, charVariants[j]);
                }
                Pop(sb, charVariants[i]);
            }
        }

        private void Pop(StringBuilder sb, string s)
        {
            sb.Remove(sb.Length - s.Length, s.Length);
        }

        private IEnumerable<string> Quote(params string[] values)
        {
            foreach (var v in values)
                yield return '"' + v + '"';
        }

        private Tuple<string, Token[]> Case(string input, params Token[] output)
        {
            return new Tuple<string, Token[]>(input, output);
        }

        private void CheckLexerOutput(IEnumerable<Token> expected, Lexer lexer)
        {
            foreach (var t in expected)
            {
                Assert.AreEqual(t.TokenType, lexer.GetCurrentToken().TokenType);
                Assert.AreEqual(t.Value, lexer.GetCurrentToken().Value);
                lexer.NextToken();
            }
        }
    }
}