using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dotson.Reading
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void AcceptCorrectBools()
        {
            var input = new[] { "true", "false" };
            foreach (var i in input)
            {
                var l = new Lexer(new StringReader(i));
                Assert.AreEqual(TokenType.Boolean, l.PeekToken().TokenType);
                Assert.AreEqual(i, l.PeekToken().Value);
            }
        }

        [TestMethod]
        public void ThrowIncorrectBools()
        {
            var input = new[] { "True", "False" };
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