using System;
using System.IO;
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
    }
}