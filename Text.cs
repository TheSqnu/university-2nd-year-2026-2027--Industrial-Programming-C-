using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace TextLab
{
    [XmlRoot("text")]
    public class Text
    {
        [XmlElement("sentence")]
        public List<Sentence> Sentences { get; set; } = new List<Sentence>();

        public void PrintConcordance()
        {
            var concordance = new Dictionary<string, ConcordanceEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var sentence in Sentences)
            {
                foreach (var word in sentence.Words)
                {
                    string key = word.Value.ToLower();
                    if (!concordance.ContainsKey(key))
                        concordance[key] = new ConcordanceEntry();

                    concordance[key].Frequency++;
                    concordance[key].LineNumbers.Add(word.LineNumber);
                }
            }

            Console.WriteLine("\nКОНКОРДАНС");
            var sortedKeys = concordance.Keys.OrderBy(k => k);
            foreach (var key in sortedKeys)
            {
                var entry = concordance[key];
                string lines = string.Join(" ", entry.LineNumbers);
                string paddedWord = key.PadRight(35, '.');
                Console.WriteLine($"{paddedWord}{entry.Frequency}: {lines}");
            }
        }
    }
}