namespace KruschJson.Writing
{
    public interface IFormatterOutput
    {
        void Write(char value);
        void Write(string value);
        void WriteIndentation(int level);
        void WriteNewline();
    }
}