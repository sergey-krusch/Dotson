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
        [TestMethod]
        public void AcceptCorrectLiterals()
        {
            var input = new[] { "true", "false", "none" };
            foreach (var i in input)
            {
                var l = new Lexer(new StringReader(i));
                Assert.AreEqual(TokenType.Literal, l.PeekToken().TokenType);
                Assert.AreEqual(i, l.PeekToken().Value);
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
                    l.PeekToken();
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
                Assert.AreEqual(TokenType.Number, l.PeekToken().TokenType);
                if (i != l.PeekToken().Value)
                {
                    l = new Lexer(new StringReader(i));
                    l.PeekToken();
                }
                Assert.AreEqual(i, l.PeekToken().Value);
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
                    l.PeekToken();
                }
                catch (Exception e)
                {
                    ee = e;
                }
                if (ee == null)
                    ee = null;
                Assert.IsNotNull(ee);
            }
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
                                sb.Remove(sb.Length - expSuffix.Length, expSuffix.Length);
                            }
                            sb.Remove(sb.Length - expPrefix.Length, expPrefix.Length);
                        }
                        sb.Remove(sb.Length - fraction.Length, fraction.Length);
                    }
                    sb.Remove(sb.Length - floor.Length, floor.Length);
                }
                sb.Remove(sb.Length - startSign.Length, startSign.Length);
            }
        }
    }
}