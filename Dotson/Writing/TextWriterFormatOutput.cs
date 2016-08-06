using System.IO;

namespace Dotson.Writing
{
    public class TextWriterFormatOutput: IFormatterOutput
    {
        private static readonly string defaultIndentation = new string(' ', 4);
        private const string defaultNewline = "\r\n";

        private readonly TextWriter textWriter;
        private readonly string indentation;
        private string newline;

        public TextWriterFormatOutput(TextWriter textWriter, string indentation, string newline)
        {
            this.textWriter = textWriter;
            this.indentation = indentation;
            this.newline = newline;
        }

        public TextWriterFormatOutput(TextWriter textWriter)
            :this(textWriter, defaultIndentation, defaultNewline)
        {
        }

        public void Write(char value)
        {
            textWriter.Write(value);
        }

        public void Write(string value)
        {
            textWriter.Write(value);
        }

        public void WriteIndentation(int level)
        {
            for (int i = 0; i < level; i ++)
                textWriter.Write(indentation);
        }

        public void WriteNewline()
        {
            textWriter.Write(newline);
        }
    }
}