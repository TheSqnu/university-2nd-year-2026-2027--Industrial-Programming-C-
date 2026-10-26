using System;
using System.Text.RegularExpressions;

namespace TextLab
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string rawText = "Mary had a little lamb! Что делает парсер? Он работает отлично.";
            var textObj = new Text();
            var curSent = new Sentence();

            // ПАРСЕР
            foreach (Match m in Regex.Matches(rawText, @"[\p{L}\d]+(?:[-'][\p{L}\d]+)*|[^\p{L}\d\s]+"))
            {
                if (char.IsLetterOrDigit(m.Value[0]))
                    curSent.Elements.Add(new Word(m.Value));
                else
                {
                    curSent.Elements.Add(new Punctuation(m.Value));
                    if (m.Value.Contains(".") || m.Value.Contains("!") || m.Value.Contains("?"))
                    {
                        textObj.Sentences.Add(curSent);
                        curSent = new Sentence();
                    }
                }
            }
            if (curSent.Elements.Count > 0) textObj.Sentences.Add(curSent);

            Console.WriteLine("=== Коммит 1: Текст успешно разбит на токены ===");
            foreach (var s in textObj.Sentences) Console.WriteLine(s);
        }
    }
}