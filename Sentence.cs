using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace TextLab
{
    public class Sentence
    {
        [XmlIgnore] 
        public List<object> Elements { get; } = new List<object>();

        [XmlElement("word")] 
        public List<Word> Words
        {
            get => Elements.OfType<Word>().ToList();
            set { Elements.Clear(); Elements.AddRange(value); }
        }

        public override string ToString() => string.Join("", Elements.Select(e => e is Word ? " " + e : e.ToString())).Trim();
    }
}