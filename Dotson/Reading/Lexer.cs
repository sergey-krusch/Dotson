using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dotson.Reading
{
    internal class Lexer
    {
        private const int surroundingRadius = 512;
        private static readonly string[] booleanLiterals = { "true", "false" };
        private const string noneLiteral = "none";

        private readonly StreamReader reader;
        private Token currentToken;
        private int currentLine = 1;
        private int currentSymbol = 1;

        public Lexer(TextReader textReader)
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(new MemoryStream()))
            {
                char[] buffer = new char[4096];
                int charCount;
                while ((charCount = textReader.Read(buffer, 0, buffer.Length)) != 0)
                    writer.Write(buffer, 0, charCount);
                writer.Flush();
                ((MemoryStream)writer.BaseStream).WriteTo(stream);
                stream.Position = 0;
            }
            reader = new StreamReader(stream);
            NextToken();
        }

        public Token GetCurrentToken()
        {
            return currentToken;
        }

        public TokenType GetCurrentTokenType()
        {
            return GetCurrentToken().TokenType;
        }

        public string GetSurroundingText()
        {
            char[] buffer = new char[surroundingRadius * 2];
            reader.BaseStream.Position = Math.Max(0, reader.BaseStream.Position - surroundingRadius);
            reader.DiscardBufferedData();
            return new string(buffer, 0, reader.ReadBlock(buffer, 0, surroundingRadius * 2));
        }

        private Exception CreateException(string message, int line, int symbol)
        {
            return new Exception(string.Format("{0} At {1}:{2}. Fragment:\n{3}", message, line, symbol, GetSurroundingText()));
        }

        public Exception CreateExceptionInCurrentPosition(string message)
        {
            return new Exception(string.Format("{0} At {1}:{2}. Fragment:\n{3}", message, currentLine, currentSymbol, GetSurroundingText()));
        }

        private Exception CreateUnterminatedStringException(int line, int symbol)
        {
            return CreateException("Unterminated string.", line, symbol);
        }

        private Exception CreateWrongNumberException(int line, int symbol)
        {
            return CreateException("Wrong number.", line, symbol);
        }

        private Exception CreateWrongBooleanException(int line, int symbol)
        {
            return CreateException("Wrong boolean.", line, symbol);
        }

        private Exception CreateWrongLiteralException(int line, int symbol)
        {
            return CreateException("Wrong literal.", line, symbol);
        }

        private bool IsNewLineChar(char c)
        {
            return c == '\u000A' || c == '\u000B' || c == '\u000C' || c == '\u000D' || c == '\u2028' || c == '\u2029';
        }

        private char? PeekChar(bool skipWhitespaces)
        {
            if (!skipWhitespaces)
            {
                int i = reader.Peek();
                if (i == -1)
                    return null;
                return (char)i;
            }
            char? pc = null;
            while (true)
            {
                int i = reader.Peek();
                if (i == -1)
                    return null;
                char c = (char) i;
                if (IsNewLineChar(c))
                {
                    if (c != '\u000A' || !pc.HasValue || pc.Value != '\u000D')
                        currentLine ++;
                    currentSymbol = 0;
                }
                else
                {
                    if (!char.IsWhiteSpace(c))
                        return c;
                }
                pc = c;
                reader.Read();
                currentSymbol++;
            }
        }

        private void NextChar()
        {
            reader.Read();
            currentSymbol++;
        }

        private Token ReadString()
        {
            int beginLine = currentLine;
            int beginSymbol = currentSymbol;
            char? c = PeekChar(false);
            if (!c.HasValue || c.Value != '"')
                throw new Exception();
            NextChar();
            StringBuilder value = new StringBuilder();
            while (true)
            {
                c = PeekChar(false);
                if (!c.HasValue)
                    throw CreateUnterminatedStringException(beginLine, beginSymbol);
                if (c.Value == '"')
                {
                    NextChar();
                    break;
                }
                if (c.Value == '\\')
                {
                    NextChar();
                    c = PeekChar(false);
                    if (!c.HasValue)
                        throw CreateUnterminatedStringException(beginLine, beginSymbol);
                    if (c.Value == 'u' || c.Value == 'U')
                    {
                        StringBuilder escapeSequence = new StringBuilder();
                        for (int i = 0; i < 4; i ++)
                        {
                            NextChar();
                            c = PeekChar(false);
                            if (!c.HasValue)
                                throw CreateUnterminatedStringException(beginLine, beginSymbol);
                            bool hexdigit = 
                                (c.Value >= '0' && c.Value <= '9') ||
                                (c.Value >= 'a' && c.Value <= 'f') ||
                                (c.Value >= 'A' && c.Value <= 'F');
                            if (!hexdigit)
                                throw new Exception("Wrong escape sequence.");
                            escapeSequence.Append(c.Value);
                        }
                        value.Append((char) Convert.ToInt32(escapeSequence.ToString(), 16));
                    }
                    else if (c.Value == '"')
                        value.Append('"');
                    else if (c.Value == '\\')
                        value.Append('\\');
                    else if (c.Value == '/')
                        value.Append('/');
                    else if (c.Value == 'b')
                        value.Append('\u0008');
                    else if (c.Value == 'f')
                        value.Append('\u000C');
                    else if (c.Value == 'n')
                        value.Append('\u000A');
                    else if (c.Value == 'r')
                        value.Append('\u000D');
                    else if (c.Value == 't')
                        value.Append('\u0009');
                    else
                        throw new Exception("Wrong escape sequence.");
                }
                else
                    value.Append(c.Value);
                NextChar();
            }
            return new Token(TokenType.String, value.ToString());
        }

        private Token ReadNumber()
        {
            int beginLine = currentLine;
            int beginSymbol = currentSymbol;
            StringBuilder valueBuilder = new StringBuilder();
            bool integer = true;
            while (true)
            {
                char? c = PeekChar(false);
                if (!c.HasValue)
                    break;
                bool numeric = 
                    (c.Value >= '0' && c.Value <= '9') || 
                    c.Value == '-' || c.Value == '.' || 
                    c.Value == 'e' || c.Value == 'E';
                if (!numeric)
                    break;
                if (c == '.')
                    integer = false;
                valueBuilder.Append(c);
                NextChar();
            }
            if (valueBuilder.Length == 0)
                throw CreateWrongNumberException(beginLine, beginSymbol);
            string value = valueBuilder.ToString();
            if (integer)
            {
                long intParseResult;
                if (!Int64.TryParse(value, out intParseResult))
                    throw CreateWrongNumberException(beginLine, beginSymbol);
                return new Token(TokenType.Integer, value);
            }
            float floatParseResult;
            if (!Single.TryParse(value, out floatParseResult))
                throw CreateWrongNumberException(beginLine, beginSymbol);
            return new Token(TokenType.Float, value);
        }

        private Token ReadBoolean()
        {
            int beginLine = currentLine;
            int beginSymbol = currentSymbol;
            char? c = PeekChar(false);
            if (!c.HasValue)
                throw CreateWrongBooleanException(beginLine, beginSymbol);
            foreach (string literal in booleanLiterals)
            {
                if (c.Value != literal[0])
                    continue;
                NextChar();
                for (int i = 1; i < literal.Length; i ++)
                {
                    c = PeekChar(false);
                    if (!c.HasValue || c.Value != literal[i])
                        throw CreateWrongBooleanException(beginLine, beginSymbol);
                    NextChar();
                }
                return new Token(TokenType.Boolean, literal);
            }
            throw CreateWrongBooleanException(beginLine, beginSymbol);
        }

        private Token ReadNone()
        {
            int beginLine = currentLine;
            int beginSymbol = currentSymbol;
            for (int i = 0; i < noneLiteral.Length; i++)
            {
                char? c = PeekChar(false);
                if (!c.HasValue || c.Value != noneLiteral[i])
                    throw CreateWrongLiteralException(beginLine, beginSymbol);
                NextChar();
            }
            return new Token(TokenType.None);
        }

        private Token InternalMoveNext()
        {
            char? c = PeekChar(true);
            if (!c.HasValue)
                return new Token(TokenType.EOF);
            switch (c)
            {
                case '{': 
                    NextChar();
                    return new Token(TokenType.DictionaryStart);
                case '}':
                    NextChar();
                    return new Token(TokenType.DictionaryEnd);
                case '[':
                    NextChar();
                    return new Token(TokenType.ArrayStart);
                case ']':
                    NextChar();
                    return new Token(TokenType.ArrayEnd);
                case ',':
                    NextChar();
                    return new Token(TokenType.Comma);
                case ':':
                    NextChar();
                    return new Token(TokenType.Colon);
                case '"':
                    return ReadString();
                case '-':
                case '.':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ReadNumber();
                case 't':
                case 'f':
                    return ReadBoolean();
                case 'n':
                    return ReadNone();
                default:
                    throw CreateException(string.Format("Unexpected symbol \"{0}\".", c), currentLine, currentSymbol);
            }
        }

        public void NextToken()
        {
            currentToken = InternalMoveNext();
        }

        public Token PeekToken()
        {
            return currentToken;
        }

    }
}