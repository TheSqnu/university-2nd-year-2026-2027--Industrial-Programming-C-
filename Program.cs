using System;
using System.Linq;
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

            Console.WriteLine("1. Сортировка по количеству слов");
            foreach (var s in textObj.Sentences.OrderBy(s => s.Words.Count))
                Console.WriteLine($"[{s.Words.Count} сл.] {s}");

            Console.WriteLine("\n2. Сортировка по длине");
            foreach (var s in textObj.Sentences.OrderBy(s => s.ToString().Length))
                Console.WriteLine($"[{s.ToString().Length} симв.] {s}");

            Console.WriteLine("\n 3. Уникальные слова (длина 6) в вопросах");
            var qWords = textObj.Sentences
                .Where(s => s.Elements.LastOrDefault() is Punctuation p && p.Value.Contains("?"))
                .SelectMany(s => s.Words).Where(w => w.Value.Length == 6)
                .Select(w => w.Value).Distinct();
            Console.WriteLine(string.Join(", ", qWords));

            Console.WriteLine("\n4 и 5. Текст после удаления слов на согласную и замен");
            string cons = "бвгджзйклмнпрстфхцчшщbcdfghjklmnpqrstvwxyz";
            foreach (var s in textObj.Sentences)
                s.Elements.RemoveAll(e => e is Word w && w.Value.Length == 6 && cons.Contains(char.ToLower(w.Value[0])));

            if (textObj.Sentences.Count > 0)
                foreach (var w in textObj.Sentences[0].Words.Where(w => w.Value.Length == 4))
                    w.Value = "ЗАМЕНА";

            foreach (var s in textObj.Sentences) Console.WriteLine(s);
        }
    }
}