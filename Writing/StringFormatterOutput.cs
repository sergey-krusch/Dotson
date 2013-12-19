using System.Text;

namespace KruschJson.Writing
{
    public class StringFormatterOutput: IFormatterOutput
    {
        private static readonly string defaultIndentation = new string(' ', 4);
        private const string defaultNewline = "\r\n";

        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly string indentation;
        private string newline;

        public StringFormatterOutput()
        {
            indentation = defaultIndentation;
            newline = defaultNewline;
        }

        public StringFormatterOutput(string indentation, string newline)
        {
            this.indentation = indentation;
            this.newline = newline;
        }

        public void Write(char value)
        {
            stringBuilder.Append(value);
        }

        public void Write(string value)
        {
            stringBuilder.Append(value);
        }

        public void WriteIndentation(int level)
        {
            for (int i = 0; i < level; i++)
                stringBuilder.Append(indentation);
        }

        public void WriteNewline()
        {
            stringBuilder.Append(newline);
        }

        public string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}