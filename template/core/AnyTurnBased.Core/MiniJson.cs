using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AnyTurnBased.Core
{
    /// <summary>
    /// Minimal JSON reader so the core stays dependency-free (it has to load in Unity Mono, IL2CPP and SMAPI).
    /// Objects become Dictionary&lt;string, object&gt;, arrays List&lt;object&gt;, numbers double.
    /// </summary>
    internal sealed class MiniJson
    {
        private readonly string _s;
        private int _i;

        private MiniJson(string s) { _s = s; }

        public static object Parse(string json)
        {
            var p = new MiniJson(json ?? throw new ArgumentNullException(nameof(json)));
            p.SkipWs();
            if (p._i < p._s.Length && p._s[p._i] == '﻿') { p._i++; p.SkipWs(); }
            var v = p.Value();
            p.SkipWs();
            if (p._i != p._s.Length) throw p.Error("trailing characters");
            return v;
        }

        private object Value()
        {
            SkipWs();
            if (_i >= _s.Length) throw Error("unexpected end");
            char c = _s[_i];
            switch (c)
            {
                case '{': return Obj();
                case '[': return Arr();
                case '"': return Str();
                case 't': Expect("true"); return true;
                case 'f': Expect("false"); return false;
                case 'n': Expect("null"); return null;
                default:
                    if (c == '-' || char.IsDigit(c)) return Num();
                    throw Error($"unexpected '{c}'");
            }
        }

        private Dictionary<string, object> Obj()
        {
            var d = new Dictionary<string, object>();
            _i++;
            SkipWs();
            if (Peek('}')) { _i++; return d; }
            while (true)
            {
                SkipWs();
                if (!Peek('"')) throw Error("expected property name");
                string key = Str();
                SkipWs();
                if (!Peek(':')) throw Error("expected ':'");
                _i++;
                d[key] = Value();
                SkipWs();
                if (Peek(',')) { _i++; continue; }
                if (Peek('}')) { _i++; return d; }
                throw Error("expected ',' or '}'");
            }
        }

        private List<object> Arr()
        {
            var l = new List<object>();
            _i++;
            SkipWs();
            if (Peek(']')) { _i++; return l; }
            while (true)
            {
                l.Add(Value());
                SkipWs();
                if (Peek(',')) { _i++; continue; }
                if (Peek(']')) { _i++; return l; }
                throw Error("expected ',' or ']'");
            }
        }

        private string Str()
        {
            var sb = new StringBuilder();
            _i++;
            while (_i < _s.Length)
            {
                char c = _s[_i++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                if (_i >= _s.Length) break;
                char e = _s[_i++];
                switch (e)
                {
                    case '"': case '\\': case '/': sb.Append(e); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (_i + 4 > _s.Length) throw Error("bad \\u escape");
                        sb.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber));
                        _i += 4;
                        break;
                    default: throw Error($"bad escape '\\{e}'");
                }
            }
            throw Error("unterminated string");
        }

        private double Num()
        {
            int start = _i;
            while (_i < _s.Length && "+-0123456789.eE".IndexOf(_s[_i]) >= 0) _i++;
            if (!double.TryParse(_s.Substring(start, _i - start), NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                throw Error("bad number");
            return v;
        }

        private void Expect(string word)
        {
            if (string.CompareOrdinal(_s, _i, word, 0, word.Length) != 0) throw Error($"expected {word}");
            _i += word.Length;
        }

        private bool Peek(char c) => _i < _s.Length && _s[_i] == c;

        private void SkipWs()
        {
            while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
        }

        private FormatException Error(string what) => new FormatException($"battle.json: {what} at character {_i}");
    }
}
