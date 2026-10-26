using System.Collections.Generic;
using System.Xml.Serialization;

namespace TextLab
{
    [XmlRoot("text")]
    public class Text
    {
        [XmlElement("sentence")]
        public List<Sentence> Sentences { get; set; } = new List<Sentence>();
    }
}