using System.Xml.Serialization;

namespace TextLab
{
    public class Word
    {
        [XmlText]
        public string Value { get; set; } = "";

        [XmlIgnore]
        public int LineNumber { get; set; }

        public Word() { }

        public Word(string v, int line)
        {
            Value = v;
            LineNumber = line;
        }

        public override string ToString() => Value;
    }
}