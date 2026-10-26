namespace TextLab
{
    public class Punctuation
    {
        public string Value { get; set; } = "";
        public Punctuation(string v) => Value = v;
        public override string ToString() => Value;
    }
}