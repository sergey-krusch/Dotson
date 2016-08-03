using System;
using System.Collections.Generic;
using System.IO;
using Dotson.Reading;
using Dotson.Writing;

namespace Dotson
{
    public class JsonNode
    {
        public static JsonNode CreateFromReader(TextReader reader)
        {
            return new Parser(reader).Root;
        }

        public static JsonNode CreateFromFile(string path)
        {
            using (TextReader reader = new StreamReader(path))
                return new Parser(reader).Root;
        }

        public static JsonNode CreateFromString(string text)
        {
            using (TextReader reader = new StringReader(text))
                return new Parser(reader).Root;
        }

        public static implicit operator JsonNode(string value)
        {
            JsonNode result = new JsonNode();
            result.Assign(value);
            return result;
        }

        public static implicit operator JsonNode(Int64 value)
        {
            JsonNode result = new JsonNode();
            result.Assign(value);
            return result;
        }

        public static implicit operator JsonNode(Single value)
        {
            JsonNode result = new JsonNode();
            result.Assign(value);
            return result;
        }

        public static implicit operator JsonNode(Boolean value)
        {
            JsonNode result = new JsonNode();
            result.Assign(value);
            return result;
        }

        private Dictionary<string, JsonNode> dictionaryData;
        private List<JsonNode> listData;
        private String stringData;
        private Int64? integerData;
        private Single? floatData;
        private Boolean? booleanData;
        
        public NodeType NodeType
        {
            get;
            private set;
        }

        public int Count
        {
            get
            {
                if (NodeType == NodeType.List)
                    return listData.Count;
                throw CreateNotApplicableException();
            }
        }

        public Dictionary<string, JsonNode>.KeyCollection Keys
        {
            get
            {
                if (NodeType != NodeType.Dictionary)
                    throw new Exception();
                return dictionaryData.Keys;
            }
        }

        public JsonNode this[int index]
        {
            get
            {
                if (NodeType != NodeType.List)
                    throw new Exception();
                return listData[index];
            }
        }

        public JsonNode this[string key]
        {
            get
            {
                if (NodeType != NodeType.Dictionary)
                    throw CreateNotApplicableException();
                return dictionaryData[key];
            }
            set
            {
                ChangeNodeType(NodeType.Dictionary);
                if (dictionaryData.ContainsKey(key))
                    dictionaryData[key] = value;
                else
                    dictionaryData.Add(key, value);
            }
        }

        public JsonNode()
        {
            NodeType = NodeType.None;
        }

        public JsonNode(NodeType nodeType)
        {
            ChangeNodeType(nodeType);
        }

        private void ChangeNodeType(NodeType newType)
        {
            if (NodeType == newType)
                return;
            if (NodeType != NodeType.None)
                throw new Exception();
            if (newType == NodeType.Dictionary)
                dictionaryData = new Dictionary<string, JsonNode>();
            if (newType == NodeType.List)
                listData = new List<JsonNode>();
            NodeType = newType;
        }

        private Exception CreateNotApplicableException()
        {
            throw new Exception("Operation is not applicable for nodes of this type.");
        }

        public void Add(JsonNode element)
        {
            ChangeNodeType(NodeType.List);
            listData.Add(element);
        }

        public void Add(string key, JsonNode element)
        {
            ChangeNodeType(NodeType.Dictionary);
            dictionaryData.Add(key, element);
        }

        public void Assign(String value)
        {
            ChangeNodeType(NodeType.String);
            stringData = value;
        }

        public void Assign(Int64 value)
        {
            ChangeNodeType(NodeType.Integer);
            integerData = value;
        }

        public void Assign(Single value)
        {
            ChangeNodeType(NodeType.Float);
            floatData = value;
        }

        public void Assign(Boolean value)
        {
            ChangeNodeType(NodeType.Boolean);
            booleanData = value;
        }

        public bool IsDictionary()
        {
            return NodeType == NodeType.Dictionary;
        }

        public bool IsList()
        {
            return NodeType == NodeType.List;
        }

        public bool IsString()
        {
            return NodeType == NodeType.String;
        }

        public bool IsInteger()
        {
            return NodeType == NodeType.Integer;
        }

        public bool IsFloat()
        {
            return NodeType == NodeType.Float;
        }

        public bool IsBoolean()
        {
            return NodeType == NodeType.Boolean;
        }

        public string AsString()
        {
            if (stringData == null)
            {
                switch (NodeType)
                {
                    case NodeType.Dictionary:
                    case NodeType.List:
                        throw CreateNotApplicableException();
                    case NodeType.Integer:
                        stringData = Convert.ToString(integerData);
                        break;
                    case NodeType.Float:
                        stringData = Convert.ToString(floatData);
                        break;
                    case NodeType.Boolean:
                        stringData = Convert.ToString(booleanData);
                        break;
                }
            }
            return stringData;
        }

        public int AsInteger()
        {
            return (int)AsLongInteger();
        }

        public uint AsUnsignedInteger()
        {
            return (uint)AsLongInteger();
        }

        public long AsLongInteger()
        {
            if (!integerData.HasValue)
            {
                switch (NodeType)
                {
                    case NodeType.Dictionary:
                    case NodeType.List:
                        throw CreateNotApplicableException();
                    case NodeType.String:
                        integerData = Convert.ToInt64(stringData);
                        break;
                    case NodeType.Float:
                        integerData = Convert.ToInt64(floatData);
                        break;
                    case NodeType.Boolean:
                        integerData = Convert.ToInt64(booleanData);
                        break;
                }
            }
            return integerData.Value;
        }

        public float AsFloat()
        {
            if (!floatData.HasValue)
            {
                switch (NodeType)
                {
                    case NodeType.Dictionary:
                    case NodeType.List:
                        throw CreateNotApplicableException();
                    case NodeType.String:
                        floatData = Convert.ToSingle(stringData);
                        break;
                    case NodeType.Integer:
                        floatData = Convert.ToSingle(integerData);
                        break;
                    case NodeType.Boolean:
                        floatData = Convert.ToSingle(booleanData);
                        break;
                }
            }
            return floatData.Value;
        }

        public bool AsBoolean()
        {
            if (!booleanData.HasValue)
            {
                switch (NodeType)
                {
                    case NodeType.Dictionary:
                    case NodeType.List:
                    case NodeType.Float:
                        throw CreateNotApplicableException();
                    case NodeType.String:
                        booleanData = Convert.ToBoolean(stringData);
                        break;
                    case NodeType.Integer:
                        booleanData = Convert.ToBoolean(integerData);
                        break;
                }
            }
            return booleanData.Value;
        }

        public T AsEnum<T>()
        {
            Type type = typeof(T);
            if (!type.IsEnum)
                throw new Exception(string.Format("{0} is not an enum type", type.FullName));
            if (NodeType != NodeType.String)
                throw CreateNotApplicableException();
            T result = (T) Enum.Parse(type, stringData, true);
            return result;
        }

        public bool ContainsKey(string key)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            return dictionaryData.ContainsKey(key);
        }

        public void EnsureChildIsList(string key)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (dictionaryData.ContainsKey(key))
            {
                JsonNode childNode = dictionaryData[key];
                if (childNode.NodeType == NodeType.List)
                    return;
                JsonNode listNode = new JsonNode(NodeType.List);
                listNode.Add(childNode);
                dictionaryData[key] = listNode;
            }
            else
                dictionaryData.Add(key, new JsonNode(NodeType.List));
        }

        public int CountInChild(string key)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return 0;
            return dictionaryData[key].Count;
        }

        public string ChildAsString(string key, string defaultValue)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return defaultValue;
            return dictionaryData[key].AsString();
        }

        public int ChildAsInteger(string key, int defaultValue)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return defaultValue;
            return dictionaryData[key].AsInteger();
        }

        public uint ChildAsUnsignedInteger(string key, uint defaultValue)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return defaultValue;
            return dictionaryData[key].AsUnsignedInteger();
        }

        public long ChildAsLongInteger(string key, long defaultValue)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return defaultValue;
            return dictionaryData[key].AsLongInteger();
        }

        public float ChildAsFloat(string key, float defaultValue)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return defaultValue;
            return dictionaryData[key].AsFloat();
        }

        public bool ChildAsBoolean(string key, bool defaultValue)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return defaultValue;
            return dictionaryData[key].AsBoolean();
        }

        public T ChildAsEnum<T>(string key, T defaultValue)
        {
            if (NodeType != NodeType.Dictionary)
                throw CreateNotApplicableException();
            if (!dictionaryData.ContainsKey(key))
                return defaultValue;
            return dictionaryData[key].AsEnum<T>();
        }

        public void SaveToWriter(TextWriter writer)
        {
            Formatter.Format(this, new TextWriterFormatOutput(writer));
        }

        public void SaveToFile(string path)
        {
            using (TextWriter writer = new StreamWriter(path))
                SaveToWriter(writer);
        }

        public string SaveToString()
        {
            StringFormatterOutput stringFormatterOutput = new StringFormatterOutput();
            Formatter.Format(this, stringFormatterOutput);
            return stringFormatterOutput.ToString();
        }
    }
}