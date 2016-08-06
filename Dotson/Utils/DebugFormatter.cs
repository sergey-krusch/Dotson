using System;
using System.Text;

namespace Dotson.Utils
{
    public class DebugFormatter
    {
        private const int levelSpacing = 4;

        private static void AppendSpaces(StringBuilder result, int level)
        {
            result.Append(new string(' ', level * levelSpacing));
        }
        
        private static void InternalFormat(StringBuilder result, JsonNode element, int level)
        {
            result.Append(Enum.GetName(typeof (NodeType), element.NodeType));
            if (element.NodeType != NodeType.None)
                result.Append(": ");
            switch (element.NodeType)
            {
                case NodeType.String:
                    result.Append(element.AsString());
                    break;
                case NodeType.Integer:
                    result.Append(element.AsInteger());
                    break;
                case NodeType.Float:
                    result.Append(element.AsFloat());
                    break;
                case NodeType.Boolean:
                    result.Append(element.AsBoolean());
                    break;
                case NodeType.List:
                    result.Append("[");
                    for (int i = 0; i < element.Count; i ++)
                    {
                        if (i != 0)
                            result.Append(",");
                        result.Append("\n");
                        AppendSpaces(result, level + 1);
                        InternalFormat(result, element[i], level + 1);
                    }
                    result.Append("\n");
                    AppendSpaces(result, level);
                    result.Append("]");
                    break;
                case NodeType.Dictionary:
                    result.Append("{");
                    bool first = true;
                    foreach (string key in element.Keys)
                    {
                        if (!first)
                            result.Append(",");
                        result.Append("\n");
                        AppendSpaces(result, level + 1);
                        result.Append(key);
                        result.Append(" => ");
                        InternalFormat(result, element[key], level + 1);
                        first = false;
                    }
                    result.Append("\n");
                    AppendSpaces(result, level);
                    result.Append("}");
                    break;
            }
        }

        public static string Format(JsonNode element)
        {
            StringBuilder result = new StringBuilder();
            InternalFormat(result, element, 0);
            return result.ToString();
        }
    }

}