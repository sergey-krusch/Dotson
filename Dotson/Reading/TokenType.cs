namespace Dotson.Reading
{
    internal enum TokenType
    {
        EOF,
        DictionaryStart,
        DictionaryEnd,
        ArrayStart,
        ArrayEnd,
        Comma,
        Colon,
        String,
        Number,
        Literal
    }
}