using System;

namespace KruschJson.Writing
{
    public class Formatter
    {
        private static void OutputString(IFormatterOutput output, string value)
        {
            output.Write('"');
            output.Write(value);
            output.Write('"');
        }

        private static void FormatNode(IFormatterOutput output, JsonNode node, int level)
        {
            switch (node.NodeType)
            {
                case NodeType.String:
                    OutputString(output, node.AsString());
                    break;
                case NodeType.Integer:
                    output.Write(Convert.ToString(node.AsLongInteger()));
                    break;
                case NodeType.Float:
                    output.Write(Convert.ToString(node.AsFloat()));
                    break;
                case NodeType.Boolean:
                    output.Write(Convert.ToString(node.AsBoolean()));
                    break;
                case NodeType.List:
                    output.Write('[');
                    for (int i = 0; i < node.Count; i++)
                    {
                        if (i != 0)
                            output.Write(',');
                        output.WriteNewline();
                        output.WriteIndentation(level + 1);
                        FormatNode(output, node[i], level + 1);
                    }
                    if (node.Count > 0)
                    {
                        output.WriteNewline();
                        output.WriteIndentation(level);
                    }
                    output.Write(']');
                    break;
                case NodeType.Dictionary:
                    output.Write('{');
                    bool first = true;
                    foreach (string key in node.Keys)
                    {
                        if (!first)
                            output.Write(',');
                        output.WriteNewline();
                        output.WriteIndentation(level + 1);
                        OutputString(output, key);
                        output.Write(':');
                        FormatNode(output, node[key], level + 1);
                        first = false;
                    }
                    if (node.Keys.Count > 0)
                    {
                        output.WriteNewline();
                        output.WriteIndentation(level);
                    }
                    output.Write('}');
                    break;
            }
        }

        public static void Format(JsonNode node, IFormatterOutput output)
        {
            FormatNode(output, node, 0);
        }
    }
}