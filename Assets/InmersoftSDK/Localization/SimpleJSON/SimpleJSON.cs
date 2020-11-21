/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * Changelog now external. See Changelog.txt
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2012-2019 Markus Göbel (Bunny83)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace InmersoftSDK.Localization.SimpleJSON
{
    public enum JsonNodeType
    {
        Array = 1,
        Object = 2,
        String = 3,
        Number = 4,
        NullValue = 5,
        Boolean = 6,
        None = 7,
        Custom = 0xFF
    }

    public enum JsonTextMode
    {
        Compact,
        Indent
    }

    public abstract partial class JsonNode
    {
        [ThreadStatic] private static StringBuilder _mEscapeBuilder;

        private static StringBuilder EscapeBuilder
        {
            get
            {
                if (_mEscapeBuilder == null)
                    _mEscapeBuilder = new StringBuilder();
                return _mEscapeBuilder;
            }
        }

        internal static string Escape(string aText)
        {
            var sb = EscapeBuilder;
            sb.Length = 0;
            if (sb.Capacity < aText.Length + aText.Length / 10)
                sb.Capacity = aText.Length + aText.Length / 10;
            foreach (var c in aText)
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    default:
                        if (c < ' ' || forceASCII && c > 127)
                        {
                            ushort val = c;
                            sb.Append("\\u").Append(val.ToString("X4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }

            var result = sb.ToString();
            sb.Length = 0;
            return result;
        }

        private static JsonNode ParseElement(string token, bool quoted)
        {
            if (quoted)
                return token;
            var tmp = token.ToLower();
            if (tmp == "false" || tmp == "true")
                return tmp == "true";
            if (tmp == "null")
                return JSONNull.CreateOrGet();
            double val;
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                return val;
            return token;
        }

        public static JsonNode Parse(string aJSON)
        {
            var stack = new Stack<JsonNode>();
            JsonNode ctx = null;
            var i = 0;
            var token = new StringBuilder();
            var tokenName = "";
            var quoteMode = false;
            var tokenIsQuoted = false;
            while (i < aJSON.Length)
            {
                switch (aJSON[i])
                {
                    case '{':
                        if (quoteMode)
                        {
                            token.Append(aJSON[i]);
                            break;
                        }

                        stack.Push(new JSONObject());
                        if (ctx != null) ctx.Add(tokenName, stack.Peek());

                        tokenName = "";
                        token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '[':
                        if (quoteMode)
                        {
                            token.Append(aJSON[i]);
                            break;
                        }

                        stack.Push(new JSONArray());
                        if (ctx != null) ctx.Add(tokenName, stack.Peek());

                        tokenName = "";
                        token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '}':
                    case ']':
                        if (quoteMode)
                        {
                            token.Append(aJSON[i]);
                            break;
                        }

                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (token.Length > 0 || tokenIsQuoted)
                            if (ctx != null)
                                ctx.Add(tokenName, ParseElement(token.ToString(), tokenIsQuoted));
                        tokenIsQuoted = false;
                        tokenName = "";
                        token.Length = 0;
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (quoteMode)
                        {
                            token.Append(aJSON[i]);
                            break;
                        }

                        tokenName = token.ToString();
                        token.Length = 0;
                        tokenIsQuoted = false;
                        break;

                    case '"':
                        quoteMode ^= true;
                        tokenIsQuoted |= quoteMode;
                        break;

                    case ',':
                        if (quoteMode)
                        {
                            token.Append(aJSON[i]);
                            break;
                        }

                        if (token.Length > 0 || tokenIsQuoted)
                            if (ctx != null)
                                ctx.Add(tokenName, ParseElement(token.ToString(), tokenIsQuoted));
                        tokenIsQuoted = false;
                        tokenName = "";
                        token.Length = 0;
                        tokenIsQuoted = false;
                        break;

                    case '\r':
                    case '\n':
                        break;

                    case ' ':
                    case '\t':
                        if (quoteMode)
                            token.Append(aJSON[i]);
                        break;

                    case '\\':
                        ++i;
                        if (quoteMode)
                        {
                            var c = aJSON[i];
                            switch (c)
                            {
                                case 't':
                                    token.Append('\t');
                                    break;
                                case 'r':
                                    token.Append('\r');
                                    break;
                                case 'n':
                                    token.Append('\n');
                                    break;
                                case 'b':
                                    token.Append('\b');
                                    break;
                                case 'f':
                                    token.Append('\f');
                                    break;
                                case 'u':
                                {
                                    var s = aJSON.Substring(i + 1, 4);
                                    token.Append((char) int.Parse(
                                        s,
                                        NumberStyles.AllowHexSpecifier));
                                    i += 4;
                                    break;
                                }
                                default:
                                    token.Append(c);
                                    break;
                            }
                        }

                        break;
                    case '/':
                        if (allowLineComments && !quoteMode && i + 1 < aJSON.Length && aJSON[i + 1] == '/')
                        {
                            while (++i < aJSON.Length && aJSON[i] != '\n' && aJSON[i] != '\r') ;
                            break;
                        }

                        token.Append(aJSON[i]);
                        break;
                    case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
                        break;

                    default:
                        token.Append(aJSON[i]);
                        break;
                }

                ++i;
            }

            if (quoteMode) throw new Exception("JSON Parse: Quotation marks seems to be messed up.");

            return ctx == null ? ParseElement(token.ToString(), tokenIsQuoted) : ctx;
        }

        #region Enumerators

        public struct Enumerator
        {
            private enum Type
            {
                None,
                Array,
                Object
            }

            private readonly Type _type;
            private Dictionary<string, JsonNode>.Enumerator _mObject;
            private List<JsonNode>.Enumerator _mArray;

            public bool IsValid => _type != Type.None;

            public Enumerator(List<JsonNode>.Enumerator aArrayEnum)
            {
                _type = Type.Array;
                _mObject = default;
                _mArray = aArrayEnum;
            }

            public Enumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum)
            {
                _type = Type.Object;
                _mObject = aDictEnum;
                _mArray = default;
            }

            public KeyValuePair<string, JsonNode> Current
            {
                get
                {
                    if (_type == Type.Array)
                        return new KeyValuePair<string, JsonNode>(string.Empty,
                            _mArray.Current);
                    if (_type == Type.Object)
                        return _mObject.Current;
                    return new KeyValuePair<string, JsonNode>(string.Empty, null);
                }
            }

            public bool MoveNext()
            {
                if (_type == Type.Array)
                    return _mArray.MoveNext();
                if (_type == Type.Object)
                    return _mObject.MoveNext();
                return false;
            }
        }

        public struct ValueEnumerator
        {
            private Enumerator m_Enumerator;

            public ValueEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(
                new Enumerator(aArrayEnum))
            {
            }

            public ValueEnumerator(
                Dictionary<string, JsonNode>.Enumerator aDictEnum) : this(
                new Enumerator(aDictEnum))
            {
            }

            public ValueEnumerator(Enumerator aEnumerator)
            {
                m_Enumerator = aEnumerator;
            }

            public JsonNode Current => m_Enumerator.Current.Value;

            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            public ValueEnumerator GetEnumerator()
            {
                return this;
            }
        }

        public struct KeyEnumerator
        {
            private Enumerator m_Enumerator;

            public KeyEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(
                new Enumerator(aArrayEnum))
            {
            }

            public KeyEnumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum)
                : this(new Enumerator(aDictEnum))
            {
            }

            public KeyEnumerator(Enumerator aEnumerator)
            {
                m_Enumerator = aEnumerator;
            }

            public string Current => m_Enumerator.Current.Key;

            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            public KeyEnumerator GetEnumerator()
            {
                return this;
            }
        }

        public class LinqEnumerator : IEnumerator<KeyValuePair<string, JsonNode>>,
            IEnumerable<KeyValuePair<string, JsonNode>>
        {
            private Enumerator m_Enumerator;
            private JsonNode m_Node;

            internal LinqEnumerator(JsonNode aNode)
            {
                m_Node = aNode;
                if (m_Node != null)
                    m_Enumerator = m_Node.GetEnumerator();
            }

            public IEnumerator<KeyValuePair<string, JsonNode>> GetEnumerator()
            {
                return new LinqEnumerator(m_Node);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new LinqEnumerator(m_Node);
            }

            public KeyValuePair<string, JsonNode> Current => m_Enumerator.Current;

            object IEnumerator.Current => m_Enumerator.Current;

            public bool MoveNext()
            {
                return m_Enumerator.MoveNext();
            }

            public void Dispose()
            {
                m_Node = null;
                m_Enumerator = new Enumerator();
            }

            public void Reset()
            {
                if (m_Node != null)
                    m_Enumerator = m_Node.GetEnumerator();
            }
        }

        #endregion Enumerators

        #region common interface

        public static bool forceASCII = false; // Use Unicode by default
        public static bool longAsString = false; // lazy creator creates a JSONString instead of JSONNumber
        public static bool allowLineComments = true; // allow "//"-style comments at the end of a line

        public abstract JsonNodeType Tag { get; }

        public virtual JsonNode this[int aIndex]
        {
            get => null;
            set { }
        }

        public virtual JsonNode this[string aKey]
        {
            get => null;
            set { }
        }

        public virtual string Value
        {
            get => "";
            set { }
        }

        public virtual int Count => 0;

        public virtual bool IsNumber => false;

        public virtual bool IsString => false;

        public virtual bool IsBoolean => false;

        public virtual bool IsNull => false;

        public virtual bool IsArray => false;

        public virtual bool IsObject => false;

        public virtual bool Inline
        {
            get => false;
            set { }
        }

        public virtual void Add(string aKey, JsonNode aItem)
        {
        }

        public virtual void Add(JsonNode aItem)
        {
            Add("", aItem);
        }

        public virtual JsonNode Remove(string aKey)
        {
            return null;
        }

        public virtual JsonNode Remove(int aIndex)
        {
            return null;
        }

        public virtual JsonNode Remove(
            JsonNode aNode)
        {
            return aNode;
        }

        public virtual JsonNode Clone()
        {
            return null;
        }

        public virtual IEnumerable<JsonNode> Children
        {
            get { yield break; }
        }

        public IEnumerable<JsonNode> DeepChildren
        {
            get
            {
                foreach (var C in Children)
                foreach (var D in C.DeepChildren)
                    yield return D;
            }
        }

        public virtual bool HasKey(string aKey)
        {
            return false;
        }

        public virtual JsonNode GetValueOrDefault(string aKey,
            JsonNode aDefault)
        {
            return aDefault;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, 0, JsonTextMode.Compact);
            return sb.ToString();
        }

        public virtual string ToString(int aIndent)
        {
            var sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, aIndent, JsonTextMode.Indent);
            return sb.ToString();
        }

        internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode);

        public abstract Enumerator GetEnumerator();

        public IEnumerable<KeyValuePair<string, JsonNode>> Linq =>
            new LinqEnumerator(this);

        public KeyEnumerator Keys => new KeyEnumerator(GetEnumerator());

        public ValueEnumerator Values => new ValueEnumerator(GetEnumerator());

        #endregion common interface

        #region typecasting properties

        public virtual double AsDouble
        {
            get
            {
                var v = 0.0;
                if (double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    return v;
                return 0.0;
            }
            set => Value = value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual int AsInt
        {
            get => (int) AsDouble;
            set => AsDouble = value;
        }

        public virtual float AsFloat
        {
            get => (float) AsDouble;
            set => AsDouble = value;
        }

        public virtual bool AsBool
        {
            get
            {
                var v = false;
                if (bool.TryParse(Value, out v))
                    return v;
                return !string.IsNullOrEmpty(Value);
            }
            set => Value = value ? "true" : "false";
        }

        public virtual long AsLong
        {
            get
            {
                long val = 0;
                if (long.TryParse(Value, out val))
                    return val;
                return 0L;
            }
            set => Value = value.ToString();
        }

        public virtual JSONArray AsArray =>
            this as JSONArray;

        public virtual JSONObject AsObject =>
            this as JSONObject;

        #endregion typecasting properties

        #region operators

        public static implicit operator JsonNode(string s)
        {
            return new JSONString(s);
        }

        public static implicit operator string(JsonNode d)
        {
            return d == null ? null : d.Value;
        }

        public static implicit operator JsonNode(double n)
        {
            return new JSONNumber(n);
        }

        public static implicit operator double(JsonNode d)
        {
            return d == null ? 0 : d.AsDouble;
        }

        public static implicit operator JsonNode(float n)
        {
            return new JSONNumber(n);
        }

        public static implicit operator float(JsonNode d)
        {
            return d == null ? 0 : d.AsFloat;
        }

        public static implicit operator JsonNode(int n)
        {
            return new JSONNumber(n);
        }

        public static implicit operator int(JsonNode d)
        {
            return d == null ? 0 : d.AsInt;
        }

        public static implicit operator JsonNode(long n)
        {
            if (longAsString)
                return new JSONString(n.ToString());
            return new JSONNumber(n);
        }

        public static implicit operator long(JsonNode d)
        {
            return d == null ? 0L : d.AsLong;
        }

        public static implicit operator JsonNode(bool b)
        {
            return new JSONBool(b);
        }

        public static implicit operator bool(JsonNode d)
        {
            return d == null ? false : d.AsBool;
        }

        public static implicit operator JsonNode(
            KeyValuePair<string, JsonNode> aKeyValue)
        {
            return aKeyValue.Value;
        }

        public static bool operator ==(JsonNode a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;
            var aIsNull = a is JSONNull || ReferenceEquals(a, null) ||
                          a is JSONLazyCreator;
            var bIsNull = b is JSONNull || ReferenceEquals(b, null) ||
                          b is JSONLazyCreator;
            if (aIsNull && bIsNull)
                return true;
            return !aIsNull && a.Equals(b);
        }

        public static bool operator !=(JsonNode a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion operators
    }
    // End of JSONNode

    public partial class JSONArray : JsonNode
    {
        private readonly List<JsonNode> m_List =
            new List<JsonNode>();

        private bool inline;

        public override bool Inline
        {
            get => inline;
            set => inline = value;
        }

        public override JsonNodeType Tag => JsonNodeType.Array;

        public override bool IsArray => true;

        public override JsonNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return new JSONLazyCreator(this);
                return m_List[aIndex];
            }
            set
            {
                if (value == null)
                    value = JSONNull.CreateOrGet();
                if (aIndex < 0 || aIndex >= m_List.Count)
                    m_List.Add(value);
                else
                    m_List[aIndex] = value;
            }
        }

        public override JsonNode this[string aKey]
        {
            get => new JSONLazyCreator(this);
            set
            {
                if (value == null)
                    value = JSONNull.CreateOrGet();
                m_List.Add(value);
            }
        }

        public override int Count => m_List.Count;

        public override IEnumerable<JsonNode> Children
        {
            get
            {
                foreach (var N in m_List)
                    yield return N;
            }
        }

        public override Enumerator GetEnumerator()
        {
            return new Enumerator(m_List.GetEnumerator());
        }

        public override void Add(string aKey, JsonNode aItem)
        {
            if (aItem == null)
                aItem = JSONNull.CreateOrGet();
            m_List.Add(aItem);
        }

        public override JsonNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                return null;
            var tmp = m_List[aIndex];
            m_List.RemoveAt(aIndex);
            return tmp;
        }

        public override JsonNode Remove(
            JsonNode aNode)
        {
            m_List.Remove(aNode);
            return aNode;
        }

        public override JsonNode Clone()
        {
            var node = new JSONArray();
            node.m_List.Capacity = m_List.Capacity;
            foreach (var n in m_List)
                if (n != null)
                    node.Add(n.Clone());
                else
                    node.Add(null);

            return node;
        }


        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append('[');
            var count = m_List.Count;
            if (inline)
                aMode = JsonTextMode.Compact;
            for (var i = 0; i < count; i++)
            {
                if (i > 0)
                    aSB.Append(',');
                if (aMode == JsonTextMode.Indent)
                    aSB.AppendLine();

                if (aMode == JsonTextMode.Indent)
                    aSB.Append(' ', aIndent + aIndentInc);
                m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }

            if (aMode == JsonTextMode.Indent)
                aSB.AppendLine().Append(' ', aIndent);
            aSB.Append(']');
        }
    }
    // End of JSONArray

    public partial class JSONObject : JsonNode
    {
        private readonly Dictionary<string, JsonNode> m_Dict =
            new Dictionary<string, JsonNode>();

        private bool inline;

        public override bool Inline
        {
            get => inline;
            set => inline = value;
        }

        public override JsonNodeType Tag => JsonNodeType.Object;

        public override bool IsObject => true;


        public override JsonNode this[string aKey]
        {
            get
            {
                if (m_Dict.ContainsKey(aKey))
                    return m_Dict[aKey];
                return new JSONLazyCreator(this, aKey);
            }
            set
            {
                if (value == null)
                    value = JSONNull.CreateOrGet();
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = value;
                else
                    m_Dict.Add(aKey, value);
            }
        }

        public override JsonNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;
                return m_Dict.ElementAt(aIndex).Value;
            }
            set
            {
                if (value == null)
                    value = JSONNull.CreateOrGet();
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return;
                var key = m_Dict.ElementAt(aIndex).Key;
                m_Dict[key] = value;
            }
        }

        public override int Count => m_Dict.Count;

        public override IEnumerable<JsonNode> Children
        {
            get
            {
                foreach (var N in m_Dict)
                    yield return N.Value;
            }
        }

        public override Enumerator GetEnumerator()
        {
            return new Enumerator(m_Dict.GetEnumerator());
        }

        public override void Add(string aKey, JsonNode aItem)
        {
            if (aItem == null)
                aItem = JSONNull.CreateOrGet();

            if (aKey != null)
            {
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = aItem;
                else
                    m_Dict.Add(aKey, aItem);
            }
            else
            {
                m_Dict.Add(Guid.NewGuid().ToString(), aItem);
            }
        }

        public override JsonNode Remove(string aKey)
        {
            if (!m_Dict.ContainsKey(aKey))
                return null;
            var tmp = m_Dict[aKey];
            m_Dict.Remove(aKey);
            return tmp;
        }

        public override JsonNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return null;
            var item = m_Dict.ElementAt(aIndex);
            m_Dict.Remove(item.Key);
            return item.Value;
        }

        public override JsonNode Remove(
            JsonNode aNode)
        {
            try
            {
                var item = m_Dict.Where(k => k.Value == aNode).First();
                m_Dict.Remove(item.Key);
                return aNode;
            }
            catch
            {
                return null;
            }
        }

        public override JsonNode Clone()
        {
            var node = new JSONObject();
            foreach (var n in m_Dict) node.Add(n.Key, n.Value.Clone());

            return node;
        }

        public override bool HasKey(string aKey)
        {
            return m_Dict.ContainsKey(aKey);
        }

        public override JsonNode GetValueOrDefault(string aKey,
            JsonNode aDefault)
        {
            JsonNode res;
            if (m_Dict.TryGetValue(aKey, out res))
                return res;
            return aDefault;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append('{');
            var first = true;
            if (inline)
                aMode = JsonTextMode.Compact;
            foreach (var k in m_Dict)
            {
                if (!first)
                    aSB.Append(',');
                first = false;
                if (aMode == JsonTextMode.Indent)
                    aSB.AppendLine();
                if (aMode == JsonTextMode.Indent)
                    aSB.Append(' ', aIndent + aIndentInc);
                aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
                if (aMode == JsonTextMode.Compact)
                    aSB.Append(':');
                else
                    aSB.Append(" : ");
                k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }

            if (aMode == JsonTextMode.Indent)
                aSB.AppendLine().Append(' ', aIndent);
            aSB.Append('}');
        }
    }
    // End of JSONObject

    public partial class JSONString : JsonNode
    {
        private string m_Data;

        public JSONString(string aData)
        {
            m_Data = aData;
        }

        public override JsonNodeType Tag => JsonNodeType.String;

        public override bool IsString => true;


        public override string Value
        {
            get => m_Data;
            set => m_Data = value;
        }

        public override Enumerator GetEnumerator()
        {
            return new Enumerator();
        }

        public override JsonNode Clone()
        {
            return new JSONString(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append('\"').Append(Escape(m_Data)).Append('\"');
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;
            var s = obj as string;
            if (s != null)
                return m_Data == s;
            var s2 = obj as JSONString;
            if (s2 != null)
                return m_Data == s2.m_Data;
            return false;
        }

        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONString

    public partial class JSONNumber : JsonNode
    {
        private double m_Data;

        public JSONNumber(double aData)
        {
            m_Data = aData;
        }

        public JSONNumber(string aData)
        {
            Value = aData;
        }

        public override JsonNodeType Tag => JsonNodeType.Number;

        public override bool IsNumber => true;

        public override string Value
        {
            get => m_Data.ToString(CultureInfo.InvariantCulture);
            set
            {
                double v;
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    m_Data = v;
            }
        }

        public override double AsDouble
        {
            get => m_Data;
            set => m_Data = value;
        }

        public override long AsLong
        {
            get => (long) m_Data;
            set => m_Data = value;
        }

        public override Enumerator GetEnumerator()
        {
            return new Enumerator();
        }

        public override JsonNode Clone()
        {
            return new JSONNumber(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append(Value);
        }

        private static bool IsNumeric(object value)
        {
            return value is int || value is uint
                                || value is float || value is double
                                || value is decimal
                                || value is long || value is ulong
                                || value is short || value is ushort
                                || value is sbyte || value is byte;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (base.Equals(obj))
                return true;
            var s2 = obj as JSONNumber;
            if (s2 != null)
                return m_Data == s2.m_Data;
            if (IsNumeric(obj))
                return Convert.ToDouble(obj) == m_Data;
            return false;
        }

        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONNumber

    public partial class JSONBool : JsonNode
    {
        private bool m_Data;

        public JSONBool(bool aData)
        {
            m_Data = aData;
        }

        public JSONBool(string aData)
        {
            Value = aData;
        }

        public override JsonNodeType Tag => JsonNodeType.Boolean;

        public override bool IsBoolean => true;

        public override string Value
        {
            get => m_Data.ToString();
            set
            {
                bool v;
                if (bool.TryParse(value, out v))
                    m_Data = v;
            }
        }

        public override bool AsBool
        {
            get => m_Data;
            set => m_Data = value;
        }

        public override Enumerator GetEnumerator()
        {
            return new Enumerator();
        }

        public override JsonNode Clone()
        {
            return new JSONBool(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append(m_Data ? "true" : "false");
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is bool)
                return m_Data == (bool) obj;
            return false;
        }

        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
    }
    // End of JSONBool

    public partial class JSONNull : JsonNode
    {
        private static readonly JSONNull m_StaticInstance =
            new JSONNull();

        public static bool reuseSameInstance = true;

        private JSONNull()
        {
        }

        public override JsonNodeType Tag => JsonNodeType.NullValue;

        public override bool IsNull => true;

        public override string Value
        {
            get => "null";
            set { }
        }

        public override bool AsBool
        {
            get => false;
            set { }
        }

        public static JSONNull CreateOrGet()
        {
            if (reuseSameInstance)
                return m_StaticInstance;
            return new JSONNull();
        }

        public override Enumerator GetEnumerator()
        {
            return new Enumerator();
        }

        public override JsonNode Clone()
        {
            return CreateOrGet();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is JSONNull;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append("null");
        }
    }
    // End of JSONNull

    internal partial class JSONLazyCreator : JsonNode
    {
        private readonly string m_Key;
        private JsonNode m_Node;

        public JSONLazyCreator(JsonNode aNode)
        {
            m_Node = aNode;
            m_Key = null;
        }

        public JSONLazyCreator(JsonNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }

        public override JsonNodeType Tag => JsonNodeType.None;

        public override JsonNode this[int aIndex]
        {
            get => new JSONLazyCreator(this);
            set => Set(new JSONArray()).Add(value);
        }

        public override JsonNode this[string aKey]
        {
            get => new JSONLazyCreator(this, aKey);
            set => Set(new JSONObject()).Add(aKey, value);
        }

        public override int AsInt
        {
            get
            {
                Set(new JSONNumber(0));
                return 0;
            }
            set => Set(new JSONNumber(value));
        }

        public override float AsFloat
        {
            get
            {
                Set(new JSONNumber(0.0f));
                return 0.0f;
            }
            set => Set(new JSONNumber(value));
        }

        public override double AsDouble
        {
            get
            {
                Set(new JSONNumber(0.0));
                return 0.0;
            }
            set => Set(new JSONNumber(value));
        }

        public override long AsLong
        {
            get
            {
                if (longAsString)
                    Set(new JSONString("0"));
                else
                    Set(new JSONNumber(0.0));
                return 0L;
            }
            set
            {
                if (longAsString)
                    Set(new JSONString(value.ToString()));
                else
                    Set(new JSONNumber(value));
            }
        }

        public override bool AsBool
        {
            get
            {
                Set(new JSONBool(false));
                return false;
            }
            set => Set(new JSONBool(value));
        }

        public override JSONArray AsArray =>
            Set(new JSONArray());

        public override JSONObject AsObject =>
            Set(new JSONObject());

        public override Enumerator GetEnumerator()
        {
            return new Enumerator();
        }

        private T Set<T>(T aVal) where T : JsonNode
        {
            if (m_Key == null)
                m_Node.Add(aVal);
            else
                m_Node.Add(m_Key, aVal);
            m_Node = null; // Be GC friendly.
            return aVal;
        }

        public override void Add(JsonNode aItem)
        {
            Set(new JSONArray()).Add(aItem);
        }

        public override void Add(string aKey, JsonNode aItem)
        {
            Set(new JSONObject()).Add(aKey, aItem);
        }

        public static bool operator ==(JSONLazyCreator a, object b)
        {
            if (b == null)
                return true;
            return ReferenceEquals(a, b);
        }

        public static bool operator !=(JSONLazyCreator a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return true;
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append("null");
        }
    }
    // End of JSONLazyCreator

    public static class JSON
    {
        public static JsonNode Parse(string aJSON)
        {
            return JsonNode.Parse(aJSON);
        }
    }
}