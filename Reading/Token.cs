namespace Dotson.Reading
{
    internal struct Token
    {
        private readonly TokenType tokenType;
        public TokenType TokenType
        {
            get
            {
                return tokenType;
            }
        }

        private readonly string value;
        public string Value
        {
            get
            {
                return value;
            }
        }

        public Token(TokenType tokenType, string value)
            : this()
        {
            this.tokenType = tokenType;
            this.value = value;
        }

        public Token(TokenType tokenType)
            : this(tokenType, null)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }

    }

}