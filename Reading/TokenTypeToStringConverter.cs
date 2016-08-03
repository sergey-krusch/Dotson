using System;
using System.Collections.Generic;

namespace Dotson.Reading
{
    internal class TokenTypeToStringConverter
    {
        private static readonly Dictionary<TokenType, string> conversions = new Dictionary<TokenType, string>();

        static TokenTypeToStringConverter()
        {
            conversions.Add(TokenType.EOF, "end of file");
            conversions.Add(TokenType.DictionaryStart, "{");
            conversions.Add(TokenType.DictionaryEnd, "}");
            conversions.Add(TokenType.ArrayStart, "[");
            conversions.Add(TokenType.ArrayEnd, "]");
            conversions.Add(TokenType.Comma, ",");
            conversions.Add(TokenType.Colon, ":");
            conversions.Add(TokenType.String, "string");
            conversions.Add(TokenType.Integer, "integer");
            conversions.Add(TokenType.Float, "float");
            conversions.Add(TokenType.Boolean, "boolean");
            conversions.Add(TokenType.None, "none");

            foreach (var value in Enum.GetValues(typeof(TokenType)))
            {
                TokenType tokenType = (TokenType)value;
                if (!conversions.ContainsKey(tokenType))
                    throw new Exception(string.Format("Missing string representation for {0} token type", Enum.GetName(typeof(TokenType), tokenType)));
            }
        }

        public static string ToString(TokenType tokenType)
        {
            return conversions[tokenType];
        }
    }

}