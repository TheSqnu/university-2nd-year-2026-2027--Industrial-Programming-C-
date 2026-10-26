using System.Collections.Generic;

namespace TextLab
{
    public class ConcordanceEntry
    {
        public int Frequency { get; set; } = 0;
        public SortedSet<int> LineNumbers { get; } = new SortedSet<int>();
    }
}