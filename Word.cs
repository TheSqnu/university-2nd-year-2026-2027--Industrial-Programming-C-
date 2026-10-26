using System.Xml.Serialization;

namespace TextLab
{
    public class Word
    {
        [XmlText]
        public string Value { get; set; } = "";

        public Word() { }
        public Word(string v) => Value = v;

        public override string ToString() => Value;
    }
}