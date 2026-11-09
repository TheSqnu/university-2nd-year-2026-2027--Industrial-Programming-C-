using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lab5csharp
{
    internal class Gift
    {
        string GiftName;
        List<Sweet> sweets;

        public Gift(String name)
        {
            this.GiftName = name;
            this.sweets = new List<Sweet>(); 
        }

        public void AddDirectly(Sweet sweet)
        {
            sweets.Add(sweet);
        }

        public string ShowGiftComposition()
        {
            if (sweets == null || sweets.Count == 0) { return "Нет сладостей"; }
            StringBuilder sb = new StringBuilder();
            sb.Append($"Состав подарка: {GiftName}\n");
            foreach (var i in sweets)
            {
                sb.Append(i.GetBriefInfo());
            }
            return sb.ToString();
        }

        public void CalculateTotalWeight()
        {
            double sum = 0;
            foreach(var i in sweets)
            {
                sum += i.GetSortingParams().weight;
            }
            Console.WriteLine($"\nВес подарка: {sum} г.");
        }
    }
}