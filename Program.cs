using System;

namespace lab5csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Gift gift = new Gift("Сладкая Радость");

            gift.AddDirectly(new Candy("Сникерс", 50, 27, 250));
            gift.AddDirectly(new Candy("Красная Шапочка", 15, 10, 80));
            gift.AddDirectly(new OtherSweet("Тульский пряник", 140, 50, 400, "Пряники"));

            Console.WriteLine(gift.ShowGiftComposition());
            gift.CalculateTotalWeight();

            Console.ReadLine();
        }
    }
}