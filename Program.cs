using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace TextLab
{
    class Program
    {
        static void Main()
        {
            string rawText =
                "Peter Piper picked a peck of pickled peppers. A peck of pickled\n" +
                "peppers Peter Piper picked. If Peter Piper picked a peck of\n" +
                "pickled peppers, where is the peck that Peter Piper picked?";

            var textObj = new Text();
            var curSent = new Sentence();

            string[] lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                int currentLineNumber = i + 1;

                foreach (Match m in Regex.Matches(lines[i], @"[\p{L}\d]+(?:[-'][\p{L}\d]+)*|[^\p{L}\d\s]+"))
                {
                    if (char.IsLetterOrDigit(m.Value[0]))
                        curSent.Elements.Add(new Word(m.Value, currentLineNumber));
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
            }
            if (curSent.Elements.Count > 0) textObj.Sentences.Add(curSent);

            Console.WriteLine("1. Сортировка по количеству слов");
            foreach (var s in textObj.Sentences.OrderBy(s => s.Words.Count)) Console.WriteLine($"[{s.Words.Count} сл.] {s}");

            Console.WriteLine("\n2. Сортировка по длине ");
            foreach (var s in textObj.Sentences.OrderBy(s => s.ToString().Length)) Console.WriteLine($"[{s.ToString().Length} симв.] {s}");

            Console.WriteLine("\n3. Уникальные слова (длина 6) в вопросах");
            var qWords = textObj.Sentences.Where(s => s.Elements.LastOrDefault() is Punctuation p && p.Value.Contains("?"))
                .SelectMany(s => s.Words).Where(w => w.Value.Length == 6).Select(w => w.Value).Distinct();
            Console.WriteLine(string.Join(", ", qWords));

            string cons = "бвгджзйклмнпрстфхцчшщbcdfghjklmnpqrstvwxyz";
            foreach (var s in textObj.Sentences)
                s.Elements.RemoveAll(e => e is Word w && w.Value.Length == 6 && cons.Contains(char.ToLower(w.Value[0])));

            if (textObj.Sentences.Count > 0)
                foreach (var w in textObj.Sentences[0].Words.Where(w => w.Value.Length == 4)) w.Value = "ЗАМЕНА";

            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists("stopwords_ru.txt"))
                stopWords.UnionWith(File.ReadAllLines("stopwords_ru.txt").Select(w => w.Trim()));
            if (File.Exists("stopwords_en.txt"))
                stopWords.UnionWith(File.ReadAllLines("stopwords_en.txt").Select(w => w.Trim()));

            foreach (var s in textObj.Sentences)
                s.Elements.RemoveAll(e => e is Word w && stopWords.Contains(w.Value));

            using var stream = new StreamWriter("output.xml");
            stream.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            var ns = new XmlSerializerNamespaces(); ns.Add("", "");
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
            using var writer = XmlWriter.Create(stream, settings);
            new XmlSerializer(typeof(Text)).Serialize(writer, textObj, ns);

            Console.WriteLine("\n--- Текст после удалений и замен ---");
            foreach (var s in textObj.Sentences) Console.WriteLine(s);
            Console.WriteLine("\nГотово! Результат сохранен в output.xml");
        }
    }
}