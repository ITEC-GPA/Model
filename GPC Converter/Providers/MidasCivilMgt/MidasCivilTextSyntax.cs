using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GPC.Converter.MidasCivil
{
    internal sealed class CivilTextLine
    {
        public int Number;
        public string Text;
        public string Record(string command) => "*" + command + " line " + Number;
        public ModelFileReadException Error(string command, string reason) => new ModelFileReadException(Record(command), reason);
    }

    internal sealed class CivilTextBlock
    {
        public string Command;
        public CivilTextLine Header;
        public string[] Arguments;
        public readonly List<CivilTextLine> Lines = new List<CivilTextLine>();
        public readonly StringBuilder Raw = new StringBuilder();
    }

    internal static class MidasCivilTextSyntax
    {
        // Commands and comments are recognized outside CSV quotes. Only a trailing backslash
        // continues a logical record; source line numbers always identify its first physical line.
        public static List<CivilTextBlock> Read(Stream source, Encoding encoding, CancellationToken token)
        {
            var blocks = new List<CivilTextBlock>();
            CivilTextBlock block = null;
            var pending = new StringBuilder(); int first = 0, number = 0; long characters = 0;
            bool ended = false;
            using (var reader = new StreamReader(source, encoding, false, 4096, true))
            {
                string raw;
                while ((raw = reader.ReadLine()) != null)
                {
                    token.ThrowIfCancellationRequested(); number++;
                    characters += raw.Length + 1;
                    if (characters > 128L * 1024 * 1024) throw new InvalidDataException("Civil text exceeds 128 MiB character limit.");
                    if (number == 1) raw = raw.TrimStart('\uFEFF');
                    var text = WithoutComment(raw, number).Trim();
                    if (text.StartsWith("*", StringComparison.Ordinal))
                    {
                        if (pending.Length != 0) throw new ModelFileReadException("Line " + first, "Unfinished continuation before command.");
                        if (ended) throw new ModelFileReadException("Line " + number, "Command after *ENDDATA.");
                        var fields = Fields(text.Substring(1), "header line " + number);
                        block = new CivilTextBlock { Command = fields[0].ToUpperInvariant(),
                            Arguments = fields, Header = new CivilTextLine { Number = number, Text = text } };
                        blocks.Add(block); ended = block.Command == "ENDDATA";
                    }
                    else if (text.Length != 0)
                    {
                        if (block == null || ended) throw new ModelFileReadException("Line " + number, "Data outside a command block.");
                        if (pending.Length == 0) first = number;
                        bool continued = text.EndsWith("\\", StringComparison.Ordinal);
                        pending.Append(continued ? text.Substring(0, text.Length - 1) + " " : text);
                        if (!continued)
                        {
                            block.Lines.Add(new CivilTextLine { Number = first, Text = pending.ToString() });
                            pending.Clear();
                        }
                    }
                    block?.Raw.Append(raw).Append('\n');
                }
            }
            if (pending.Length != 0) throw new ModelFileReadException("Line " + first, "Unfinished continuation at end of file.");
            if (!ended) throw new ModelFileReadException("EOF line " + number, "Missing *ENDDATA: incomplete Civil model file.");
            return blocks;
        }

        private static string WithoutComment(string text, int line)
        {
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '"')
                {
                    if (quoted && i + 1 < text.Length && text[i + 1] == '"') { i++; continue; }
                    quoted = !quoted;
                }
                else if (!quoted && text[i] == ';') return text.Substring(0, i);
            }
            if (quoted) throw new ModelFileReadException("Line " + line, "Unclosed quote.");
            return text;
        }

        public static string[] Fields(string text, string record)
        {
            var fields = new List<string>(); var value = new StringBuilder();
            bool quoted = false, closed = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"')
                {
                    if (quoted && i + 1 < text.Length && text[i + 1] == '"') { value.Append('"'); i++; }
                    else if (quoted) { quoted = false; closed = true; }
                    else if (closed || value.ToString().Trim().Length != 0) throw new ModelFileReadException(record, "Misplaced quote.");
                    else { value.Clear(); quoted = true; }
                }
                else if (c == ',' && !quoted) { fields.Add(value.ToString().Trim()); value.Clear(); closed = false; }
                else if (closed && !char.IsWhiteSpace(c)) throw new ModelFileReadException(record, "Characters after closing quote.");
                else value.Append(c);
            }
            if (quoted) throw new ModelFileReadException(record, "Unclosed quote.");
            fields.Add(value.ToString().Trim()); return fields.ToArray();
        }
    }
}
