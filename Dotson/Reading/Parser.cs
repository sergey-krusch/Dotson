using System;
using System.IO;
using System.Text;

namespace Dotson.Reading
{
    internal class Parser
    {
        private readonly Lexer lexer;
        private JsonNode root;

        public JsonNode Root
        {
            get
            {
                return root ?? (root = Parse());
            }
        }

        public Parser(TextReader textReader)
        {
            lexer = new Lexer(textReader);
        }

        private Exception CreateException(string message)
        {
            return lexer.CreateExceptionInCurrentPosition(message);
        }

        private Exception CreateExpectedException(Token actual, TokenType[] expected)
        {
            StringBuilder message = new StringBuilder();
            message.Append("Unexpected token: ").Append('"').Append(TokenTypeToStringConverter.ToString(actual.TokenType)).Append('"').Append('.');
            if (expected != null)
            {
                message.Append(" Expected: ");
                for (int i = 0; i < expected.Length; i ++)
                {
                    if (i != 0)
                        message.Append(" or ");
                    message.Append('"').Append(TokenTypeToStringConverter.ToString(expected[i])).Append('"');
                }
            }
            message.Append(".");
            return CreateException(message.ToString());
        }

        private Exception CreateExpectedException(Token actual, TokenType expected)
        {
            return CreateExpectedException(actual, new TokenType[] { expected });
        }

        private JsonNode ParseDictionary()
        {
            if (lexer.GetCurrentTokenType() != TokenType.DictionaryStart)
                throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.DictionaryStart);
            JsonNode result = new JsonNode(NodeType.Dictionary);
            lexer.NextToken();
            while (true)
            {
                if (lexer.GetCurrentTokenType() == TokenType.DictionaryEnd)
                {
                    lexer.NextToken();
                    break;
                }
                if (lexer.GetCurrentTokenType() != TokenType.String)
                    throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.String);
                string key = lexer.GetCurrentToken().Value;
                lexer.NextToken();
                if (lexer.GetCurrentTokenType() != TokenType.Colon)
                    throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.Colon);
                lexer.NextToken();
                JsonNode child = Parse();
                result.Add(key, child);
                if (lexer.GetCurrentTokenType() != TokenType.Comma)
                {
                    if (lexer.GetCurrentTokenType() == TokenType.DictionaryEnd)
                    {
                        lexer.NextToken();
                        break;
                    }
                    throw CreateExpectedException(lexer.GetCurrentToken(), new TokenType[] { TokenType.DictionaryEnd, TokenType.Comma });
                }
                lexer.NextToken();
            }
            return result;
        }

        private JsonNode ParseList()
        {
            if (lexer.GetCurrentTokenType() != TokenType.ArrayStart)
                throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.ArrayStart);
            JsonNode result = new JsonNode(NodeType.List);
            lexer.NextToken();
            while (true)
            {
                if (lexer.GetCurrentTokenType() == TokenType.ArrayEnd)
                {
                    lexer.NextToken();
                    break;
                }
                JsonNode child = Parse();
                result.Add(child);
                if (lexer.GetCurrentTokenType() != TokenType.Comma)
                {
                    if (lexer.GetCurrentTokenType() == TokenType.ArrayEnd)
                    {
                        lexer.NextToken();
                        break;
                    }
                    throw CreateExpectedException(lexer.GetCurrentToken(), new TokenType[] { TokenType.ArrayEnd, TokenType.Comma });
                }
                lexer.NextToken();
            }
            return result;
        }

        private JsonNode ParseString()
        {
            if (lexer.GetCurrentTokenType() != TokenType.String)
                throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.String);
            JsonNode result = new JsonNode(NodeType.String);
            result.Assign(lexer.GetCurrentToken().Value);
            lexer.NextToken();
            return result;
        }

        private JsonNode ParseInteger()
        {
            if (lexer.GetCurrentTokenType() != TokenType.Integer)
                throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.Integer);
            JsonNode result = new JsonNode(NodeType.Integer);
            result.Assign(Convert.ToInt64(lexer.GetCurrentToken().Value));
            lexer.NextToken();
            return result;
        }

        private JsonNode ParseFloat()
        {
            if (lexer.GetCurrentTokenType() != TokenType.Float)
                throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.Float);
            JsonNode result = new JsonNode(NodeType.Float);
            result.Assign(Convert.ToSingle(lexer.GetCurrentToken().Value));
            lexer.NextToken();
            return result;
        }

        private JsonNode ParseLiteral()
        {
            if (lexer.GetCurrentTokenType() != TokenType.Literal)
                throw CreateExpectedException(lexer.GetCurrentToken(), TokenType.Literal);
            string value = lexer.GetCurrentToken().Value;
            JsonNode result;
            if (value == Consts.NoneLiteral)
                result = new JsonNode(NodeType.None);
            else
            {
                result = new JsonNode(NodeType.Boolean);
                result.Assign(value == Consts.TrueLiteral);
            }
            lexer.NextToken();
            return result;
        }

        private JsonNode Parse()
        {
            if (lexer.GetCurrentTokenType() == TokenType.EOF)
                return new JsonNode();
            if (lexer.GetCurrentTokenType() == TokenType.DictionaryStart)
                return ParseDictionary();
            if (lexer.GetCurrentTokenType() == TokenType.ArrayStart)
                return ParseList();
            if (lexer.GetCurrentTokenType() == TokenType.String)
                return ParseString();
            if (lexer.GetCurrentTokenType() == TokenType.Integer)
                return ParseInteger();
            if (lexer.GetCurrentTokenType() == TokenType.Float)
                return ParseFloat();
            if (lexer.GetCurrentTokenType() == TokenType.Literal)
                return ParseLiteral();
            throw new Exception();
        }

    }
}