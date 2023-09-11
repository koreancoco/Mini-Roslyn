using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using MiniRoslyn.CodeAnalysis.Text;

namespace MiniRoslyn.Tests.CodeAnalysis.Text
{
    public class AnnotatedText
    {
        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }


        public static AnnotatedText Parse(string text)
        {
            var unIndentText = UnIndentText(text);
            var textBuilder = new StringBuilder();
            var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
            var startStack = new Stack<int>();
            int position = 0;
            // [] pair包围的内容表示期望的位置
            foreach (var c in unIndentText)
            {
                if (c == '[')
                {
                    startStack.Push(position);
                }
                else if (c == ']')
                {
                    if (startStack.Count == 0)
                        throw new ArgumentException("Too many ']' in text");
                    var start = startStack.Pop();
                    var end = position;
                    var span = TextSpan.FromBounds(start, end);
                    spanBuilder.Add(span);
                }
                else
                {
                    position++;
                    textBuilder.Append(c);
                }
            }
            
            
            return new AnnotatedText(textBuilder.ToString(), spanBuilder.ToImmutable());
        }

        private static string UnIndentText(string text)
        {
            var unIndentLines = UnIndentLines(text);
            return string.Join(Environment.NewLine, unIndentLines);
        }

        public static string[] UnIndentLines(string text)
        {
            // 取出所有行
            List<string> lines = new List<string>();
            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) !=null)
                {
                    lines.Add(line);
                }
            }

            // 计算最小公共indent
            int minIndentLen = int.MaxValue;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.Trim().Length == 0)
                {
                    lines[i] = string.Empty;
                    continue;
                }

                int indentLen = line.Length - line.TrimStart().Length;
                minIndentLen = Math.Min(indentLen, minIndentLen);
            }

            // 移除公共indent
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line == string.Empty)
                {
                    continue;
                }

                lines[i] = lines[i].Substring(minIndentLen);
            }
            
            // 移除首位空行，保留文本中间空行
            while (lines.Count > 0 && lines[0] == string.Empty)
            {
                lines.RemoveAt(0);
            }
            
            while (lines.Count - 1 >= 0 && lines[lines.Count - 1] == string.Empty)
            {
                lines.RemoveAt(lines.Count - 1);
            }


            return lines.ToArray();
        }
    }
}